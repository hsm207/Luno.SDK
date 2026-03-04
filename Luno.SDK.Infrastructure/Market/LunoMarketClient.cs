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
public class LunoMarketClient : ILunoMarketClient
{
    private readonly HttpClient _httpClient;
    private readonly LunoTelemetry _telemetry;
    private readonly ILogger _logger;
    private readonly LunoApiClient _apiClient;
    private readonly string _apiVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketClient"/> class.
    /// </summary>
    /// <param name="options">Optional client configuration options.</param>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    public LunoMarketClient(LunoClientOptions? options, HttpClient httpClient)
    {
        options ??= new LunoClientOptions();
        _apiVersion = options.ApiVersion;
        _logger = options.LoggerFactory.CreateLogger<LunoMarketClient>();
        _telemetry = new LunoTelemetry();
        _httpClient = httpClient;

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
}
