using System.Diagnostics;
using System.Diagnostics.Metrics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Luno.SDK;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
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

    private ILunoClient CreateClient()
    {
        var options = new LunoClientOptions { BaseUrl = _server.Url! };
        return new LunoClient(options);
    }

    private static ActivityListener CaptureActivity(string operationName, ManualResetEventSlim signal, Action<Activity> onCapture)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == LunoInstrumentation.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity =>
            {
                if (activity.OperationName == operationName)
                {
                    onCapture(activity);
                    signal.Set();
                }
            }
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    [Fact(DisplayName = "Given standard DI container, When resolving ILunoClient, Then return a valid instance with all sub-clients configured.")]
    public void Resolve_ContainerIsConfigured_ReturnsValidClient()
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
    public async Task GetTickers_ApiSucceeds_EmitsTelemetry()
    {
        // Arrange
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

        var client = CreateClient();

        Activity? capturedActivity = null;
        using var activityStoppedEvent = new ManualResetEventSlim();
        using var listener = CaptureActivity(operationName, activityStoppedEvent, activity => capturedActivity = activity);

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
        var results = new List<Luno.SDK.Application.Market.TickerResponse>();
        await foreach (var ticker in client.Market.GetTickersAsync())
        {
            results.Add(ticker);
        }

        // Wait for the telemetry activity to physically stop
        activityStoppedEvent.Wait(TimeSpan.FromSeconds(2));

        // Assert
        Assert.Single(results);
        Assert.NotNull(capturedActivity);
        Assert.Equal(operationName, capturedActivity.OperationName);
        Assert.Equal("Success", capturedActivity.GetTagItem("luno.status"));
        Assert.True(hasSuccessMeasurement);
    }

    [Fact(DisplayName = "Given the Luno API returns a 500 error, When fetching tickers, Then bubble up the correct LunoApiException AND emit an error trace signal.")]
    public async Task GetTickers_ApiFails_BubblesExceptionAndEmitsErrorTrace()
    {
        // Arrange
        var operationName = "GetMarketTickers";
        _server.Given(Request.Create().WithPath("/api/1/tickers").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500).WithBody("Internal Server Error"));

        var client = CreateClient();

        Activity? capturedActivity = null;
        using var activityStoppedEvent = new ManualResetEventSlim();
        using var activityListener = CaptureActivity(operationName, activityStoppedEvent, activity => capturedActivity = activity);

        // Act & Assert
        await Assert.ThrowsAsync<LunoApiException>(async () =>
        {
            await foreach (var _ in client.Market.GetTickersAsync()) { }
        });

        // Wait for the telemetry activity to physically stop
        activityStoppedEvent.Wait(TimeSpan.FromSeconds(2));

        Assert.NotNull(capturedActivity);
        Assert.Equal("Error", capturedActivity.GetTagItem("luno.status"));
    }

    [Fact(DisplayName = "Given the Luno API returns quirky JSON with missing mandatory fields, When fetching tickers, Then ensure the mapping fails immediately.")]
    public async Task GetTickers_ApiReturnsQuirkyJson_FailsFast()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/api/1/tickers").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    tickers = new[] { new { pair = "XBTZAR", timestamp = 1772555388322 } }
                }));

        var client = CreateClient();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoMappingException>(async () =>
        {
            await foreach (var ticker in client.Market.GetTickersAsync())
            {
                // Should throw on first iteration
            }
        });
        
        Assert.Contains("null", ex.Message);
    }

    [Fact(DisplayName = "Given a successful API request for a single pair, When fetching ticker, Then verify real telemetry and proper mapping.")]
    [Trait("Category", "Integration")]
    public async Task GetTickerAsync_Success_ReturnsTickerResponse()
    {
        // Arrange
        var operationName = "GetMarketTicker";

        _server.Given(Request.Create().WithPath("/api/1/ticker").WithParam("pair", "XBTZAR").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    pair = "XBTZAR",
                    timestamp = 1772555388322,
                    bid = "1000000",
                    ask = "1000100",
                    last_trade = "1000050",
                    rolling_24_hour_volume = "500",
                    status = "ACTIVE"
                }));

        var client = CreateClient();

        Activity? capturedActivity = null;
        using var activityStoppedEvent = new ManualResetEventSlim();
        using var listener = CaptureActivity(operationName, activityStoppedEvent, activity => capturedActivity = activity);

        // Act
        var result = await client.Market.GetTickerAsync("XBTZAR");

        // Wait for the telemetry activity to physically stop
        activityStoppedEvent.Wait(TimeSpan.FromSeconds(2));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000050m, result.Price);
        Assert.True(result.IsActive);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1772555388322), result.Timestamp);

        Assert.NotNull(capturedActivity);
        Assert.Equal(operationName, capturedActivity.OperationName);
        Assert.Equal("Success", capturedActivity.GetTagItem("luno.status"));
    }

    [Theory(DisplayName = "Given various API errors, When fetching ticker, Then bubble up the correct domain exception and emit an error trace.")]
    [Trait("Category", "Integration")]
    [InlineData(500, typeof(LunoApiException))]
    [InlineData(503, typeof(LunoApiException))]
    [InlineData(401, typeof(LunoUnauthorizedException))]
    [InlineData(403, typeof(LunoForbiddenException))]
    [InlineData(429, typeof(LunoRateLimitException))]
    public async Task GetTickerAsync_ApiFails_BubblesExceptionAndEmitsErrorTrace(int statusCode, Type expectedExceptionType)
    {
        // Arrange
        var operationName = "GetMarketTicker";
        _server.Given(Request.Create().WithPath("/api/1/ticker").WithParam("pair", "XBTZAR").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(statusCode).WithBody("Error response"));

        var client = CreateClient();

        Activity? capturedActivity = null;
        using var activityStoppedEvent = new ManualResetEventSlim();
        using var listener = CaptureActivity(operationName, activityStoppedEvent, activity => capturedActivity = activity);

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, async () =>
        {
            await client.Market.GetTickerAsync("XBTZAR");
        });

        // Wait for the telemetry activity to physically stop
        activityStoppedEvent.Wait(TimeSpan.FromSeconds(2));

        Assert.NotNull(capturedActivity);
        Assert.Equal(operationName, capturedActivity.OperationName);
        Assert.Equal("Error", capturedActivity.GetTagItem("luno.status"));
    }

    [Fact(DisplayName = "Given the Luno API returns quirky JSON with missing mandatory fields for a single ticker, When fetching ticker, Then ensure the mapping fails immediately.")]
    public async Task GetTickerAsync_QuirkyJson_FailsFast()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/api/1/ticker").WithParam("pair", "XBTZAR").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    pair = "XBTZAR",
                    timestamp = 1772555388322
                }));

        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<LunoMappingException>(() => client.Market.GetTickerAsync("XBTZAR"));
    }
}
