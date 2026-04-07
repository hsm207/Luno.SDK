using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Account;
using Luno.SDK.Application.Account;

namespace Luno.SDK;

/// <summary>
/// Provides fluent extension methods for Luno Account operations.
/// </summary>
public static class LunoAccountExtensions
{
    /// <summary>
    /// Asynchronously fetches a list of account balances.
    /// </summary>
    /// <param name="client">The <see cref="ILunoAccountClient"/> instance to use for the request.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing an <see cref="IReadOnlyList{AccountBalanceResponse}"/>.</returns>
    public static Task<IReadOnlyList<AccountBalanceResponse>> GetBalancesAsync(
        this ILunoAccountClient client,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<GetBalancesQuery, IReadOnlyList<AccountBalanceResponse>>(new GetBalancesQuery(), ct);
    }

    /// <summary>
    /// Asynchronously fetches a list of account balances for specific assets.
    /// </summary>
    /// <param name="client">The <see cref="ILunoAccountClient"/> instance to use for the request.</param>
    /// <param name="assets">List of assets to filter by (e.g., "XBT", "ETH").</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing an <see cref="IReadOnlyList{AccountBalanceResponse}"/>.</returns>
    public static Task<IReadOnlyList<AccountBalanceResponse>> GetBalancesAsync(
        this ILunoAccountClient client,
        IEnumerable<string> assets,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<GetBalancesQuery, IReadOnlyList<AccountBalanceResponse>>(
            new GetBalancesQuery { Assets = assets?.ToArray() }, ct);
    }
}
