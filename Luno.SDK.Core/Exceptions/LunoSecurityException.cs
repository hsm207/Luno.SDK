using System;

namespace Luno.SDK;

/// <summary>
/// The base exception for all security-related errors (Authentication, Authorization).
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrUnauthorised, ErrInsufficientPerms, ErrApiKeyRevoked, ErrIncorrectPin
/// </remarks>
[Serializable]
public abstract class LunoSecurityException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    protected LunoSecurityException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected LunoSecurityException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoSecurityException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoSecurityException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
