using Microsoft.Kiota.Abstractions;
using Luno.SDK.Market;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Infrastructure.Generated;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoMarketClient"/> interface using the Kiota-generated API client.
/// </summary>
/// <param name="requestAdapter">The request adapter used to communicate with the Luno API.</param>
internal class LunoMarketClient(IRequestAdapter requestAdapter) : ILunoMarketClient
{
    private readonly LunoApiClient _apiClient = new(requestAdapter);

    /// <inheritdoc />
    public async IAsyncEnumerable<Ticker> GetTickersAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var response = await _apiClient.Api.One.Tickers.GetAsync(req =>
            req.Options.Add(new LunoTelemetryOptions("GetMarketTickers")), ct);

        // Trust the API response to have the Tickers array if successful
        foreach (var dto in response!.Tickers!)
        {
            yield return Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity(dto);
        }
    }

    /// <inheritdoc />
    public async Task<Ticker> GetTickerAsync(string pair, CancellationToken ct = default)
    {
        var response = await _apiClient.Api.One.Ticker.GetAsync(req =>
        {
            req.QueryParameters.Pair = pair;
            req.Options.Add(new LunoTelemetryOptions("GetMarketTicker"));
        }, ct);

        return Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity(response!);
    }
}
