namespace Luno.SDK.Trading;

/// <summary>
/// Specifies how long an order remains active before it is executed or expires.
/// </summary>
public abstract record TimeInForce
{
    private TimeInForce() { }

    /// <summary>Good 'Til Cancelled. The order remains open until it is filled or cancelled.</summary>
    public sealed record GtcType : TimeInForce;

    /// <summary>Immediate Or Cancel. The part that cannot be filled immediately is cancelled.</summary>
    public sealed record IocType : TimeInForce;

    /// <summary>Fill Or Kill. The order must be filled immediately and completely or cancelled.</summary>
    public sealed record FokType : TimeInForce;

    /// <summary>A singleton instance representing Good 'Til Cancelled.</summary>
    public static readonly TimeInForce GTC = new GtcType();

    /// <summary>A singleton instance representing Immediate Or Cancel.</summary>
    public static readonly TimeInForce IOC = new IocType();

    /// <summary>A singleton instance representing Fill Or Kill.</summary>
    public static readonly TimeInForce FOK = new FokType();
}
