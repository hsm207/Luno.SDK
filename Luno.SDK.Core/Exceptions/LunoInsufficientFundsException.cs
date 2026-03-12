using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when an operation cannot be completed due to insufficient funds or balance.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrInsufficientFunds, ErrInsufficientBalance
/// </remarks>
[Serializable]
public class LunoInsufficientFundsException : LunoBusinessRuleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoInsufficientFundsException"/> class.
    /// </summary>
    public LunoInsufficientFundsException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoInsufficientFundsException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoInsufficientFundsException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoInsufficientFundsException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoInsufficientFundsException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoInsufficientFundsException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoInsufficientFundsException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
