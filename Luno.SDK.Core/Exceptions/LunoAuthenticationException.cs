using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// Exception thrown during the authentication process.
/// </summary>
[Serializable]
public class LunoAuthenticationException : LunoSecurityException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAuthenticationException"/> class.
    /// </summary>
    public LunoAuthenticationException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoAuthenticationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoAuthenticationException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAuthenticationException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoAuthenticationException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
