using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when an order is rejected by the exchange due to limits or risk rules.
/// </summary>
/// <remarks>
/// Primary Luno error codes: ErrAmountTooSmall, ErrAmountTooBig, ErrPriceTooHigh, ErrPriceTooLow, ErrVolumeTooLow, ErrVolumeTooHigh, ErrValueTooHigh, ErrInvalidPrice, ErrInvalidVolume, ErrInvalidOrderSide, ErrCannotStopUnknownOrNonPendingOrder, ErrNoTradesToInferStopDirection, ErrStopPriceTooHigh, ErrStopPriceTooLow, ErrInvalidStopDirection, ErrInvalidStopPrice, ErrNotEnoughLiquidity, ErrPostOnlyNotAllowed, ErrOrderCanceled, ErrPriceDenominationNotAllowed, ErrVolumeDenominationNotAllowed.
/// </remarks>
[Serializable]
public class LunoOrderRejectedException : LunoBusinessRuleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoOrderRejectedException"/> class.
    /// </summary>
    public LunoOrderRejectedException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoOrderRejectedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoOrderRejectedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoOrderRejectedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoOrderRejectedException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoOrderRejectedException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoOrderRejectedException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
