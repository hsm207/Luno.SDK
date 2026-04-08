using Luno.SDK.Account;
using Luno.SDK.Infrastructure.Generated;

namespace Luno.SDK.Infrastructure.Account;

/// <summary>
/// Provides a concrete implementation of the account clients using the generated Kiota client.
/// </summary>
public class LunoAccountClient(LunoApiClient api, ILunoRequestDispatcher requests) : ILunoAccountClient, ILunoAccountOperations
{
    /// <inheritdoc />
    public ILunoRequestDispatcher Requests { get; } = requests;

    /// <inheritdoc />
    async Task<IReadOnlyList<Balance>> ILunoAccountOperations.FetchBalancesAsync(string[]? assets, CancellationToken ct)
    {
        var response = await api.Api.One.Balance.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Assets = assets;
            requestConfiguration.Options.Add(new Luno.SDK.Infrastructure.Telemetry.LunoTelemetryOptions("GetBalances"));
        }, ct).ConfigureAwait(false);

        return response!.Balance!.Select(b => b!.ToDomain()).ToList().AsReadOnly();
    }
}
