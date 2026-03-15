namespace Luno.SDK.Trading;

/// <summary>
/// Represents the high-level life cycle state of an order on the Luno exchange.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order is awaiting to enter the order book.
    /// </summary>
    Awaiting,

    /// <summary>
    /// The order is active in the order book (partially filled or untouched).
    /// </summary>
    Pending,

    /// <summary>
    /// The order is no longer active (filled or cancelled).
    /// </summary>
    Complete
}
