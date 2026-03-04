using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Luno.SDK.Core.Market;
using Luno.SDK.Infrastructure.Telemetry;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoMarketClientTests : IDisposable
{
    private readonly WireMockServer _server;

    public LunoMarketClientTests()
    {
        _server = WireMockServer.Start();
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    private static LunoMarketClient CreateClient(HttpClient httpClient, LunoTelemetry telemetry)
    {
        var baseAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
        var decoratedAdapter = new LunoTelemetryAdapter(baseAdapter, telemetry, NullLogger.Instance);
        return new LunoMarketClient(decoratedAdapter);
    }

    [Fact(DisplayName = "Given standard DI container, When resolving ILunoClient, Then return a valid instance with all sub-clients configured.")]
    public void ResolveWhenContainerIsConfiguredShouldReturnValidClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLunoClient();
        using var provider = services.BuildServiceProvider();

        // Act
        var client = provider.GetService<ILunoClient>();

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.Market);
    }

    [Fact(DisplayName = "Given a successful API request, When fetching tickers, Then verify that real OpenTelemetry traces and metrics are emitted.")]
    public async Task GetTickersWhenApiSucceedsShouldEmitTelemetry()
    {
        // Arrange
        var telemetry = new LunoTelemetry();
        var operationName = "GetMarketTickers";

        _server.Given(Request.Create().WithPath("/api/1/tickers").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    tickers = new[]
                    {
                        new
                        {
                            pair = "XBTZAR",
                            timestamp = 1772555388322,
                            bid = "1000000",
                            ask = "1000100",
                            last_trade = "1000050",
                            rolling_24_hour_volume = "500",
                            status = "ACTIVE"
                        }
                    }
                }));

        using var httpClient = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var client = CreateClient(httpClient, telemetry);

        Activity? capturedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = (source) => source.Name == LunoInstrumentation.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStopped = (activity) => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);

        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == LunoInstrumentation.Name)
                listener.EnableMeasurementEvents(instrument);
        };

        bool hasSuccessMeasurement = false;
        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "luno.sdk.requests")
            {
                foreach (var tag in tags)
                {
                    if (tag.Key == "luno.status" && (string)tag.Value! == "Success")
                        hasSuccessMeasurement = true;
                }
            }
        });
        meterListener.Start();

        // Act
        var results = new List<Ticker>();
        await foreach (var ticker in client.GetTickersAsync())
        {
            results.Add(ticker);
        }

        // Assert
        Assert.Single(results);
        Assert.NotNull(capturedActivity);
        Assert.Equal(operationName, capturedActivity.OperationName);
        Assert.Equal("Success", capturedActivity.GetTagItem("luno.status"));
        Assert.True(hasSuccessMeasurement);
    }

    [Fact(DisplayName = "Given the Luno API returns a 500 error, When fetching tickers, Then bubble up the correct ApiException AND emit an error trace signal.")]
    public async Task GetTickersWhenApiFailsShouldBubbleExceptionAndEmitErrorTrace()
    {
        // Arrange
        var telemetry = new LunoTelemetry();
        _server.Given(Request.Create().WithPath("/api/1/tickers").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500).WithBody("Internal Server Error"));

        using var httpClient = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var client = CreateClient(httpClient, telemetry);

        Activity? capturedActivity = null;
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = (source) => source.Name == LunoInstrumentation.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStopped = (activity) => capturedActivity = activity
        };
        ActivitySource.AddActivityListener(activityListener);

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(async () =>
        {
            await foreach (var _ in client.GetTickersAsync()) { }
        });

        Assert.NotNull(capturedActivity);
        Assert.Equal("Error", capturedActivity.GetTagItem("luno.status"));
    }

    [Fact(DisplayName = "Given the Luno API returns a successful response with null tickers, When fetching tickers, Then throw InvalidOperationException.")]
    public async Task GetTickersWhenApiReturnsNullTickersShouldThrowInvalidOperationException()
    {
        // Arrange
        var telemetry = new LunoTelemetry();
        _server.Given(Request.Create().WithPath("/api/1/tickers").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { tickers = (object[]?)null }));

        using var httpClient = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var client = CreateClient(httpClient, telemetry);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in client.GetTickersAsync()) { }
        });

        Assert.Contains("missing or null", ex.Message);
    }

    [Fact(DisplayName = "Given the Luno API returns quirky JSON with missing optional fields, When fetching tickers, Then ensure the Kiota engine handles it AND records success.")]
    public async Task GetTickersWhenApiReturnsQuirkyJsonShouldHandleGracefully()
    {
        // Arrange
        var telemetry = new LunoTelemetry();
        _server.Given(Request.Create().WithPath("/api/1/tickers").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    tickers = new[] { new { pair = "XBTZAR", timestamp = 1772555388322 } }
                }));

        using var httpClient = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        var client = CreateClient(httpClient, telemetry);

        // Act
        var results = new List<Ticker>();
        await foreach (var ticker in client.GetTickersAsync())
        {
            results.Add(ticker);
        }

        // Assert
        Assert.Single(results);
        Assert.Equal("XBTZAR", results[0].Pair);
        Assert.Equal(MarketStatus.Unknown, results[0].Status);
    }
}
