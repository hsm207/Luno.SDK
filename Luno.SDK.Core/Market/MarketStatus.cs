namespace Luno.SDK.Market;

/// <summary>
/// Represents the operational status of a Luno market pair.
/// </summary>
public enum MarketStatus
{
    /// <summary>
    /// The market is active and open for trading.
    /// </summary>
    Active,

    /// <summary>
    /// The market is in post-only mode.
    /// </summary>
    PostOnly,

    /// <summary>
    /// The market is disabled and closed for trading.
    /// </summary>
    Disabled,

    /// <summary>
    /// The market is temporarily suspended due to volatility. 
    /// New orders can only be posted as post-only.
    /// </summary>
    Suspended,

    /// <summary>
    /// The market status is unknown or not recognized by the current SDK version.
    /// </summary>
    Unknown
}
