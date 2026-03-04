using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using Luno.SDK;
using Luno.SDK.Core.Market;
using Luno.SDK.Infrastructure.Telemetry;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
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

    [Fact(DisplayName = "Given a successful API request, When fetching tickers, Then verify that real OpenTelemetry traces and metrics are emitted with correct luno.operation and luno.status tags.")]
    public async Task GetTickersAsync_GivenASuccessfulApiRequest_WhenFetchingTickers_ThenVerifyThatRealOpenTelemetryTracesAndMetricsAreEmittedWithCorrectLunoOperationAndLunoStatusTags()
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

        // MeterListener to verify counter
        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == LunoInstrumentation.Name)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        bool hasSuccessMeasurement = false;
        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "luno.sdk.requests")
            {
                foreach (var tag in tags)
                {
                    if (tag.Key == "luno.status" && (string)tag.Value! == "Success")
                    {
                        hasSuccessMeasurement = true;
                    }
                }
            }
        });
        meterListener.Start();

        // Act
        var tickers = new List<Ticker>();
        await foreach (var ticker in client.GetTickersAsync())
        {
            tickers.Add(ticker);
        }

        // Assert
        Assert.Single(tickers);
        Assert.NotNull(capturedActivity);
        Assert.Equal(operationName, capturedActivity.OperationName);
        Assert.Equal(operationName, capturedActivity.GetTagItem("luno.operation"));
        Assert.Equal("Success", capturedActivity.GetTagItem("luno.status"));
        Assert.True(hasSuccessMeasurement, "Expected a success measurement in telemetry.");
    }

    [Fact(DisplayName = "Given the Luno API returns a 500 error, When fetching tickers, Then bubble up the correct ApiException AND emit an error trace signal.")]
    public async Task GetTickersAsync_GivenTheLunoApiReturnsA500Error_WhenFetchingTickers_ThenBubbleUpTheCorrectApiExceptionAndEmitAnErrorTraceSignal()
    {
        // Arrange
        var telemetry = new LunoTelemetry();
        var operationName = "GetMarketTickers";
        
        _server.Given(Request.Create().WithPath("/api/1/tickers").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(500)
                .WithBody("Internal Server Error"));

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

        // MeterListener to verify counter
        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == LunoInstrumentation.Name)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        bool hasErrorMeasurement = false;
        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "luno.sdk.requests")
            {
                foreach (var tag in tags)
                {
                    if (tag.Key == "luno.status" && (string)tag.Value! == "Error")
                    {
                        hasErrorMeasurement = true;
                    }
                }
            }
        });
        meterListener.Start();

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(async () =>
        {
            await foreach (var _ in client.GetTickersAsync()) { }
        });

        Assert.True(hasErrorMeasurement, "Expected an error measurement in telemetry.");
        Assert.NotNull(capturedActivity);
        Assert.Equal(operationName, capturedActivity.OperationName);
        Assert.Equal(operationName, capturedActivity.GetTagItem("luno.operation"));
        Assert.Equal("Error", capturedActivity.GetTagItem("luno.status"));
    }

    [Fact(DisplayName = "Given the Luno API returns quirky JSON with missing optional fields, When fetching tickers, Then ensure the Kiota engine handles it AND records a successful operation trace.")]
    public async Task GetTickersAsync_GivenTheLunoApiReturnsQuirkyJsonWithMissingOptionalFields_WhenFetchingTickers_ThenEnsureTheKiotaEngineHandlesItAndRecordsASuccessfulOperationTrace()
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
                            timestamp = 1772555388322
                            // Missing bid, ask, last_trade, rolling_24_hour_volume, status
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

        // MeterListener to verify counter
        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == LunoInstrumentation.Name)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        bool hasSuccessMeasurement = false;
        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "luno.sdk.requests")
            {
                foreach (var tag in tags)
                {
                    if (tag.Key == "luno.status" && (string)tag.Value! == "Success")
                    {
                        hasSuccessMeasurement = true;
                    }
                }
            }
        });
        meterListener.Start();

        // Act
        var tickers = new List<Ticker>();
        await foreach (var ticker in client.GetTickersAsync())
        {
            tickers.Add(ticker);
        }

        // Assert
        Assert.Single(tickers);
        var resultTicker = tickers[0];
        Assert.Equal("XBTZAR", resultTicker.Pair);
        Assert.Equal(0m, resultTicker.Bid);
        Assert.Equal(0m, resultTicker.Ask);
        Assert.Equal(0m, resultTicker.LastTrade);
        Assert.Equal(0m, resultTicker.Rolling24HourVolume);
        Assert.Equal(MarketStatus.Unknown, resultTicker.Status);

        Assert.NotNull(capturedActivity);
        Assert.Equal(operationName, capturedActivity.OperationName);
        Assert.Equal(operationName, capturedActivity.GetTagItem("luno.operation"));
        Assert.Equal("Success", capturedActivity.GetTagItem("luno.status"));
        Assert.True(hasSuccessMeasurement, "Expected a success measurement in telemetry.");
    }
}
