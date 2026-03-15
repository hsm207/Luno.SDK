using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Account;

/// <summary>
/// Defines the contract for Luno Account operations.
/// </summary>
public interface ILunoAccountClient
{
    /// <summary>
    /// Gets the command dispatcher used to orchestrate account application-layer logic.
    /// </summary>
    ILunoCommandDispatcher Commands { get; }

    /// <summary>
    /// Asynchronously fetches a list of account balances.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> containing a read-only list of <see cref="Balance"/>.</returns>
    Task<IReadOnlyList<Balance>> FetchBalancesAsync(CancellationToken ct = default);
}
