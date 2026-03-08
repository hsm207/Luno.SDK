namespace Luno.SDK.Account;

/// <summary>
/// Defines operations for interacting with Luno accounts and balances.
/// </summary>
public interface ILunoAccountClient
{
    /// <summary>
    /// Gets a static snapshot of all balances for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A read-only list of balances.</returns>
    Task<IReadOnlyList<Balance>> GetBalancesAsync(CancellationToken cancellationToken = default);
}
