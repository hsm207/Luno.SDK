namespace Luno.SDK.Trading;

/// <summary>
/// Side of the trigger price to activate a stop-limit order.
/// </summary>
public enum StopDirection
{
    /// <summary>
    /// Direction is automatically inferred based on the last trade price and the stop price.
    /// If last trade price is less than stop price then stop direction is ABOVE otherwise is BELOW.
    /// </summary>
    RelativeLastTrade,

    /// <summary>
    /// Activates when the market price goes above the trigger price.
    /// </summary>
    Above,

    /// <summary>
    /// Activates when the market price goes below the trigger price.
    /// </summary>
    Below
}
