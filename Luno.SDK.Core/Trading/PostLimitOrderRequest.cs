using System;

namespace Luno.SDK.Trading;

/// <summary>
/// Represents a request to place an idempotent limit order on the Luno exchange.
/// </summary>
public class PostLimitOrderRequest
{
    /// <summary>
    /// The currency pair to trade (e.g., "XBTZAR").
    /// </summary>
    public string Pair { get; set; } = string.Empty;

    /// <summary>
    /// The type of the order (BID or ASK).
    /// </summary>
    public OrderType Type { get; set; }

    /// <summary>
    /// Amount of cryptocurrency to buy or sell.
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// The limit price for the order.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The base currency Account ID.
    /// </summary>
    public long? BaseAccountId { get; set; }

    /// <summary>
    /// The counter currency Account ID.
    /// </summary>
    public long? CounterAccountId { get; set; }

    /// <summary>
    /// Unique UUID for deduplication. Used for idempotency.
    /// </summary>
    public string? ClientOrderId { get; set; }

    /// <summary>
    /// Specifies how long the order remains active. Default is usually GTC.
    /// </summary>
    public TimeInForce? TimeInForce { get; set; }

    /// <summary>
    /// If true, the order is cancelled if it would trade immediately.
    /// </summary>
    public bool? PostOnly { get; set; }

    /// <summary>
    /// Trigger price for stop-limit orders.
    /// </summary>
    public decimal? StopPrice { get; set; }

    /// <summary>
    /// Side of the trigger price to activate the order.
    /// </summary>
    public StopDirection? StopDirection { get; set; }

    /// <summary>
    /// Unix timestamp in milliseconds for request validity.
    /// </summary>
    public long? Timestamp { get; set; }

    /// <summary>
    /// Milliseconds after timestamp for request expiration.
    /// </summary>
    public long? TTL { get; set; }
}
