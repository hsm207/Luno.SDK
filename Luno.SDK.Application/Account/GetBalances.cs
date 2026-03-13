using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Account;

namespace Luno.SDK.Application.Account;

/// <summary>
/// The list of all Accounts and their respective balances for the requesting user.
/// </summary>
/// <remarks>Permissions required: Perm_R_Balance</remarks>
public record GetBalancesQuery;

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

        return balances.Select(b => b.ToResponse()).ToList().AsReadOnly();
    }
}
