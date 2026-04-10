namespace Luno.SDK.Trading;

/// <summary>
/// Represents a calculated limit order quote that enforces volume and price invariants.
/// </summary>
public record OrderQuote(
    string Pair,
    OrderSide Side,
    decimal Volume,
    decimal Price,
    string QuoteCurrency)
{
    /// <summary>
    /// Gets the raw calculated gross value of the trade in the Quote currency (Volume * Price).
    /// </summary>
    public decimal GrossQuoteValue => Volume * Price;

    /// <summary>
    /// Gets the estimated maximum total budget required in the Quote currency for this trade.
    /// Evaluates to <see cref="GrossQuoteValue"/> for Buy orders; 0 for Sell orders.
    /// </summary>
    public decimal EstimatedCost => Side == OrderSide.Buy ? GrossQuoteValue : 0m;

    /// <summary>
    /// Gets the estimated total proceeds (before fees) expected in the Quote currency for this trade.
    /// Evaluates to <see cref="GrossQuoteValue"/> for Sell orders; 0 for Buy orders.
    /// </summary>
    public decimal EstimatedProceeds => Side == OrderSide.Sell ? GrossQuoteValue : 0m;
}
