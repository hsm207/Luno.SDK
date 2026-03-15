using Microsoft.Kiota.Abstractions;
using Luno.SDK.Market;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Infrastructure.Generated;
using System.Runtime.CompilerServices;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoMarketClient"/> interface using the Kiota-generated API client.
/// </summary>
/// <param name="api">The generated Kiota API client.</param>
/// <param name="commands">The command dispatcher for the application layer.</param>
public class LunoMarketClient(LunoApiClient api, ILunoCommandDispatcher commands) : ILunoMarketClient
{
    private readonly LunoApiClient _apiClient = api; // Changed to use the injected api

    /// <inheritdoc />
    public ILunoCommandDispatcher Commands { get; } = commands; // Added Commands property

    /// <inheritdoc />
    public async IAsyncEnumerable<Ticker> FetchTickersAsync(
        [EnumeratorCancellation] CancellationToken ct = default) // Changed return type to Ticker, and added using for EnumeratorCancellation
    {
        var response = await _apiClient.Api.One.Tickers.GetAsync(req =>
            req.Options.Add(new LunoTelemetryOptions("GetMarketTickers")), ct);

        var tickers = response?.Tickers
            ?? throw new LunoMappingException("API returned a null tickers collection.", "TickersResponse");

        foreach (var dto in tickers)
        {
            yield return Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity(dto);
        }
    }

    /// <inheritdoc />
    public async Task<Ticker> FetchTickerAsync(string pair, CancellationToken ct = default)
    {
        var response = await _apiClient.Api.One.Ticker.GetAsync(req =>
        {
            req.QueryParameters.Pair = pair;
            req.Options.Add(new LunoTelemetryOptions("GetMarketTicker"));
        }, ct);

        if (response == null)
            throw new LunoMappingException("API returned a null ticker response.", "GetTickerResponse");

        return Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity(response);
    }
}
