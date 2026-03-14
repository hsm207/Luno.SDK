namespace Luno.SDK.Trading;

/// <summary>
/// Represents the response returned after successfully placing or finding an order.
/// </summary>
public class OrderResponse
{
    /// <summary>
    /// The unique identifier of the order on the exchange.
    /// </summary>
    public string OrderId { get; set; } = string.Empty;
}
