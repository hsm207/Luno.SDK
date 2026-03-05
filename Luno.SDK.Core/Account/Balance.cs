namespace Luno.SDK.Account;

/// <summary>
/// Represents a high-fidelity snapshot of an account balance.
/// </summary>
public record Balance
{
    /// <summary>
    /// Unique identifier for the account.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Currency code (e.g., "XBT", "ETH").
    /// </summary>
    public required string Asset { get; init; }

    /// <summary>
    /// Amount available to send or trade.
    /// </summary>
    public required decimal Available { get; init; }

    /// <summary>
    /// Amount locked by Luno.
    /// </summary>
    public required decimal Reserved { get; init; }

    /// <summary>
    /// Amount awaiting verification.
    /// </summary>
    public required decimal Unconfirmed { get; init; }

    /// <summary>
    /// User-defined name for the account.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Calculated property: Available + Reserved.
    /// </summary>
    public decimal Total => Available + Reserved;
}
