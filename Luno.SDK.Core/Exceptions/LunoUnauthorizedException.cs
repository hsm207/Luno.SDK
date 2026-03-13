using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when the API returns a 401 Unauthorized status code, indicating invalid credentials.
/// </summary>
/// <remarks>
/// Primary Luno error codes: ErrUnauthorised, ErrApiKeyRevoked, ErrIncorrectPin.
/// </remarks>
[Serializable]
public class LunoUnauthorizedException : LunoSecurityException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoUnauthorizedException"/> class.
    /// </summary>
    public LunoUnauthorizedException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoUnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoUnauthorizedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoUnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoUnauthorizedException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoUnauthorizedException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoUnauthorizedException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
