namespace Luno.SDK.Trading;

/// <summary>
/// Side of the trigger price to activate a stop-limit order.
/// </summary>
public abstract record StopDirection
{
    private StopDirection() { }

    /// <summary>Direction is automatically inferred based on the last trade price and the stop price.</summary>
    public sealed record RelativeLastTradeDirection : StopDirection;

    /// <summary>Activates when the market price goes above the trigger price.</summary>
    public sealed record AboveDirection : StopDirection;

    /// <summary>Activates when the market price goes below the trigger price.</summary>
    public sealed record BelowDirection : StopDirection;

    /// <summary>A singleton instance representing automatic inference.</summary>
    public static readonly StopDirection RelativeLastTrade = new RelativeLastTradeDirection();

    /// <summary>A singleton instance representing the ABOVE trigger direction.</summary>
    public static readonly StopDirection Above = new AboveDirection();

    /// <summary>A singleton instance representing the BELOW trigger direction.</summary>
    public static readonly StopDirection Below = new BelowDirection();
}
