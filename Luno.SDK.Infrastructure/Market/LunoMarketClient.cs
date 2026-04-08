using Microsoft.Kiota.Abstractions;
using Luno.SDK.Market;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Infrastructure.Generated;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the market clients using the Kiota-generated API client.
/// </summary>
public class LunoMarketClient(LunoApiClient api, ILunoRequestDispatcher requests) : ILunoMarketClient, ILunoMarketOperations
{
    private readonly LunoApiClient _apiClient = api; // Changed to use the injected api

    /// <inheritdoc />
    public ILunoRequestDispatcher Requests { get; } = requests;

    /// <inheritdoc />
    async IAsyncEnumerable<Ticker> ILunoMarketOperations.FetchTickersAsync(
        string[]? pairs,
        [EnumeratorCancellation] CancellationToken ct) // Changed return type to Ticker, and added using for EnumeratorCancellation
    {
        var response = await _apiClient.Api.One.Tickers.GetAsync(req =>
        {
            req.QueryParameters.Pair = pairs;
            req.Options.Add(new LunoTelemetryOptions("GetMarketTickers"));
        }, ct);
        
        foreach (var dto in response!.Tickers!)
        {
            yield return Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity(dto);
        }
    }

    /// <inheritdoc />
    async Task<Ticker> ILunoMarketOperations.FetchTickerAsync(string pair, CancellationToken ct)
    {
        var response = await _apiClient.Api.One.Ticker.GetAsync(req =>
        {
            req.QueryParameters.Pair = pair;
            req.Options.Add(new LunoTelemetryOptions("GetMarketTicker"));
        }, ct);

        return Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity(response!);
    }

    /// <inheritdoc />
    async Task<IReadOnlyList<MarketInfo>> ILunoMarketOperations.FetchMarketsAsync(string[]? pairs, CancellationToken ct)
    {
        var response = await _apiClient.Api.Exchange.One.Markets.GetAsync(req =>
        {
            req.QueryParameters.Pair = pairs;
            req.Options.Add(new LunoTelemetryOptions("GetMarkets"));
        }, ct);

        return response!.Markets!
            .Select(Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity)
            .ToList()
            .AsReadOnly();
    }
}
