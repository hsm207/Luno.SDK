using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Account;

/// <summary>
/// Defines the low-level data-fetching operations for Luno Account.
/// This interface is used by handlers to avoid a circular dependency on the command dispatcher.
/// </summary>
public interface ILunoAccountOperations
{
    /// <summary>
    /// Asynchronously fetches a list of account balances.
    /// </summary>
    /// <param name="assets">Only return balances for wallets with these currencies (optional).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> containing a read-only list of <see cref="Balance"/>.</returns>
    Task<IReadOnlyList<Balance>> FetchBalancesAsync(string[]? assets = null, CancellationToken ct = default);
}

/// <summary>
/// Defines the full contract for Luno Account operations, including command dispatching.
/// </summary>
public interface ILunoAccountClient : ILunoAccountOperations
{
    /// <summary>
    /// Gets the command dispatcher used to orchestrate account application-layer logic.
    /// </summary>
    ILunoCommandDispatcher Commands { get; }
}
