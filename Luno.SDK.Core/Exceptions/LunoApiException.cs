using System;

namespace Luno.SDK;

/// <summary>
/// The base exception for all server-side API errors returned by Luno.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrInternal
/// </remarks>
[Serializable]
public class LunoApiException : LunoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoApiException"/> class.
    /// </summary>
    public LunoApiException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoApiException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoApiException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoApiException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoApiException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoApiException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoApiException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
