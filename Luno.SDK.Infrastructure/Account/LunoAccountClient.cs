using Luno.SDK.Account;
using Luno.SDK.Infrastructure.Generated;

namespace Luno.SDK.Infrastructure.Account;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoAccountClient"/> interface
/// using the generated Kiota client.
/// </summary>
/// <param name="api">The generated Kiota API client.</param>
/// <param name="commands">The command dispatcher for the application layer.</param>
public class LunoAccountClient(LunoApiClient api, ILunoCommandDispatcher commands) : ILunoAccountClient
{
    /// <inheritdoc />
    public ILunoCommandDispatcher Commands { get; } = commands;

    /// <inheritdoc />
    public async Task<IReadOnlyList<Balance>> FetchBalancesAsync(CancellationToken ct = default)
    {
        var response = await api.Api.One.Balance.GetAsync(requestConfiguration =>
        {
            requestConfiguration.Options.Add(new Luno.SDK.Infrastructure.Telemetry.LunoTelemetryOptions("GetBalances"));
        }, ct).ConfigureAwait(false);

        return response?.Balance?.Where(b => b != null).Select(b => b!.ToDomain()).ToList().AsReadOnly()
            ?? throw new LunoMappingException("API returned a null balances collection.", "BalanceResponse");
    }
}
