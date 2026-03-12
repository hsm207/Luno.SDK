using System;

namespace Luno.SDK;

/// <summary>
/// The base exception for maintenance and restrictive market modes.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrUnderMaintenance, ErrMarketUnavailable, ErrPostOnlyMode, ErrMarketNotAllowed, ErrCannotTradeWhileQuoteActive
/// </remarks>
[Serializable]
public abstract class LunoMarketStateException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class.
    /// </summary>
    protected LunoMarketStateException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected LunoMarketStateException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoMarketStateException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoMarketStateException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
