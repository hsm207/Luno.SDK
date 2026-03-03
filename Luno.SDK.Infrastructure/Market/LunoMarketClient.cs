using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions.Authentication;
using Luno.SDK.Core.Market;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Infrastructure.Generated;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoMarketClient"/> interface using the Kiota-generated API client.
/// </summary>
public class LunoMarketClient : ILunoMarketClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LunoTelemetry _telemetry;
    private readonly ILogger _logger;
    private readonly bool _disposeClient;
    private readonly LunoApiClient _apiClient;
    private readonly string _apiVersion;

    private record RawTickersResponse(
        [property: JsonPropertyName("tickers")] IEnumerable<RawTickerDto> Tickers
    );

    private record RawTickerDto(
        [property: JsonPropertyName("pair")] string? Pair,
        [property: JsonPropertyName("ask")] string? Ask,
        [property: JsonPropertyName("bid")] string? Bid,
        [property: JsonPropertyName("last_trade")] string? LastTrade,
        [property: JsonPropertyName("rolling_24_hour_volume")] string? Rolling24HourVolume,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("timestamp")] long? Timestamp
    );

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketClient"/> class.
    /// </summary>
    /// <param name="options">Optional client configuration options.</param>
    /// <param name="httpClient">Optional existing HTTP client to use for requests.</param>
    public LunoMarketClient(LunoClientOptions? options = null, HttpClient? httpClient = null)
    {
        options ??= new LunoClientOptions();
        _apiVersion = options.ApiVersion;
        _logger = options.LoggerFactory.CreateLogger<LunoMarketClient>();
        _telemetry = new LunoTelemetry();
        
        if (httpClient != null)
        {
            _httpClient = httpClient;
            _disposeClient = false;
        }
        else
        {
            var handler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(options.BaseUrl) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
            _disposeClient = true;
        }

        var auth = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(auth, httpClient: _httpClient);
        _apiClient = new LunoApiClient(adapter);
    }

    internal LunoMarketClient(HttpClient httpClient, LunoTelemetry telemetry, ILogger logger, string apiVersion = "1")
    {
        _httpClient = httpClient;
        _telemetry = telemetry;
        _logger = logger;
        _apiVersion = apiVersion;
        _disposeClient = false;

        var auth = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(auth, httpClient: _httpClient);
        _apiClient = new LunoApiClient(adapter);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<Ticker> GetTickersAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        const string operation = "GetTickers";
        using var activity = _telemetry.ActivitySource.StartActivity(operation);
        
        var stopwatch = Stopwatch.StartNew();
        _logger.LogDebug("Fetching market tickers via Kiota client.");

        var requestBuilder = _apiVersion switch {
            "1" => _apiClient.Api.One.Tickers,
            _ => throw new NotSupportedException($"API version {_apiVersion} is not supported by this client version.")
        };

        Luno.SDK.Infrastructure.Generated.Models.ListTickersResponse? response = null;
        try
        {
            response = await requestBuilder.GetAsync(cancellationToken: ct);
            _telemetry.RecordRequest(operation, "Success");
        }
        catch (Exception ex)
        {
            _telemetry.RecordRequest(operation, "Error");
            _logger.LogError(ex, "Failed to fetch market tickers from the API.");
            throw;
        }
        finally
        {
            _telemetry.RecordDuration(stopwatch.Elapsed.TotalMilliseconds, operation);
        }

        if (response?.Tickers is null) yield break;

        foreach (var dto in response.Tickers)
        {
            yield return Luno.SDK.Infrastructure.Market.LunoMapper.MapToEntity(dto);
        }
    }

    /// <summary>
    /// Disposes the underlying HTTP client and telemetry resources if they were owned by this instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposeClient)
        {
            _httpClient.Dispose();
            _telemetry.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
