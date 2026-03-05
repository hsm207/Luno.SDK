using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Core.Account;

namespace Luno.SDK.Application.Account;

/// <summary>
/// The list of all Accounts and their respective balances for the requesting user.
/// </summary>
/// <remarks>Permissions required: Perm_R_Balance</remarks>
public record GetBalancesQuery;

/// <summary>
/// Represents the application-layer response containing balance data for a specific account.
/// </summary>
/// <param name="AccountId">ID of the account.</param>
/// <param name="Asset">Currency code for the asset held in this account.</param>
/// <param name="Available">The amount available to send or trade.</param>
/// <param name="Reserved">Amount locked by Luno and cannot be sent or traded. This could be due to open orders.</param>
/// <param name="Unconfirmed">Amount that is awaiting some sort of verification to be credited to this account. This could be an on-chain transaction that Luno is waiting for further block verifications to happen.</param>
/// <param name="Total">The calculated total amount (Available + Reserved).</param>
/// <param name="Name">The name set by the user upon creating the account.</param>
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
    /// The list of all Accounts and their respective balances for the requesting user.
    /// </summary>
    /// <remarks>Permissions required: Perm_R_Balance</remarks>
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
