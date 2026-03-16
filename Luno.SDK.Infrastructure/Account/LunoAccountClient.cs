using Luno.SDK.Account;
using Luno.SDK.Infrastructure.Generated;

namespace Luno.SDK.Infrastructure.Account;

/// <summary>
/// Provides a concrete implementation of the account clients using the generated Kiota client.
/// </summary>
public class LunoAccountClient(LunoApiClient api, ILunoCommandDispatcher commands) : ILunoAccountClient
{
    /// <inheritdoc />
    public ILunoCommandDispatcher Commands { get; } = commands;

    /// <inheritdoc />
    public async Task<IReadOnlyList<Balance>> FetchBalancesAsync(string[]? assets = null, CancellationToken ct = default)
    {
        var response = await api.Api.One.Balance.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Assets = assets;
            requestConfiguration.Options.Add(new Luno.SDK.Infrastructure.Telemetry.LunoTelemetryOptions("GetBalances"));
        }, ct).ConfigureAwait(false);

        return response!.Balance!.Select(b => b!.ToDomain()).ToList().AsReadOnly();
    }
}
