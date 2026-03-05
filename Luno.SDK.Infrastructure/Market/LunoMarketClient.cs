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

        if (response?.Tickers is null)
        {
            throw new LunoMappingException("The API response was successful but the tickers array was missing or null.", "GetTickersResponse");
        }

        foreach (var dto in response.Tickers)
        {
            yield return Luno.SDK.Infrastructure.Market.MarketMapper.MapToEntity(dto);
        }
    }
}
