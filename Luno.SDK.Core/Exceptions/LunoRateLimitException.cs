using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when the API rate limit is exceeded.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrTooManyRequests, ErrAddressCreateRateLimitReached, ErrActiveCryptoRequestExists, ErrMaxActiveFiatRequestsExists
/// </remarks>
[Serializable]
public class LunoRateLimitException : LunoApiException
{
    /// <summary>
    /// The number of seconds to wait before retrying the request.
    /// </summary>
    public int? RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoRateLimitException"/> class.
    /// </summary>
    public LunoRateLimitException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoRateLimitException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoRateLimitException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoRateLimitException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoRateLimitException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoRateLimitException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="retryAfter">The number of seconds to wait before retrying the request.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoRateLimitException(string message, string? errorCode, int? statusCode, int? retryAfter = null, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException)
    {
        RetryAfter = retryAfter;
    }
}
