using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when the API returns a 403 Forbidden status code, indicating insufficient permissions.
/// </summary>
[Serializable]
public class LunoForbiddenException : LunoSecurityException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoForbiddenException"/> class.
    /// </summary>
    public LunoForbiddenException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoForbiddenException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoForbiddenException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoForbiddenException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoForbiddenException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
