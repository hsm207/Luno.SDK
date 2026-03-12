using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// The base exception for violations of trading or market rules.
/// </summary>
[Serializable]
public class LunoBusinessRuleException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoBusinessRuleException"/> class.
    /// </summary>
    public LunoBusinessRuleException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoBusinessRuleException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoBusinessRuleException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoBusinessRuleException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoBusinessRuleException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoBusinessRuleException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoBusinessRuleException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
