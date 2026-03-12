using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// The base exception for all security-related errors (Authentication, Authorization).
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrUnauthorised, ErrInsufficientPerms, ErrApiKeyRevoked, ErrIncorrectPin
/// </remarks>
[Serializable]
public class LunoSecurityException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    public LunoSecurityException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoSecurityException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoSecurityException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoSecurityException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
    }

