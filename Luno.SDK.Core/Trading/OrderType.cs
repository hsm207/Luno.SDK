namespace Luno.SDK.Trading;

/// <summary>
/// Specifies the behavioral type of an order on the Luno exchange.
/// Aligned with Luno V2 API ontology (LIMIT/MARKET/STOP_LIMIT).
/// </summary>
public enum OrderType
{
    /// <summary>
    /// A limit order with a specified price and volume.
    /// </summary>
    Limit,

    /// <summary>
    /// A market order executed at the best available price.
    /// </summary>
    Market,

    /// <summary>
    /// A stop-limit order triggered when a stop price condition is met.
    /// </summary>
    StopLimit
}
