namespace Luno.SDK.Trading;

/// <summary>
/// Specifies how long an order remains active before it is executed or expires.
/// </summary>
public enum TimeInForce
{
    /// <summary>
    /// Good 'Til Cancelled. The order remains open until it is filled or cancelled by the user.
    /// </summary>
    GTC,

    /// <summary>
    /// Immediate Or Cancel. The part of the order that cannot be filled immediately will be cancelled. Cannot be post-only.
    /// </summary>
    IOC,

    /// <summary>
    /// Fill Or Kill. If the order cannot be filled immediately and completely it will be cancelled before any trade. Cannot be post-only.
    /// </summary>
    FOK
}
