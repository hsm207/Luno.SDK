using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Core.Account;

namespace Luno.SDK.Application.Account;

/// <summary>
/// Represents a query to get all current account balances.
/// </summary>
public record GetBalancesQuery;

/// <summary>
/// Represents the application-layer response containing balance data for a specific account.
/// </summary>
/// <param name="AccountId">The unique identifier for the account.</param>
/// <param name="Asset">The currency code (e.g., XBT).</param>
/// <param name="Available">The amount available to send or trade.</param>
/// <param name="Reserved">The amount locked by Luno.</param>
/// <param name="Unconfirmed">The amount awaiting verification.</param>
/// <param name="Total">The calculated total amount (Available + Reserved).</param>
/// <param name="Name">The user-defined name for the account.</param>
public record AccountBalanceResponse(
    string AccountId,
    string Asset,
    decimal Available,
    decimal Reserved,
    decimal Unconfirmed,
    decimal Total,
    string Name
);

/// <summary>
/// Orchestrates the retrieval of account balances from the Luno API.
/// </summary>
/// <param name="accountClient">The specialized account client used to fetch core balance entities.</param>
public class GetBalancesHandler(ILunoAccountClient accountClient)
{
    /// <summary>
    /// Handles the request to get balances and returns them as application-layer DTOs.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A list of <see cref="AccountBalanceResponse"/> objects.</returns>
    public async Task<IReadOnlyList<AccountBalanceResponse>> HandleAsync(
        GetBalancesQuery query,
        CancellationToken ct = default)
    {
        var balances = await accountClient.GetBalancesAsync(ct).ConfigureAwait(false);

        return balances.Select(b => new AccountBalanceResponse(
            b.AccountId,
            b.Asset,
            b.Available,
            b.Reserved,
            b.Unconfirmed,
            b.Total,
            b.Name
        )).ToList().AsReadOnly();
    }
}
