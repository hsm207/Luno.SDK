using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown for 409 Conflict / duplicate ID errors.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrDuplicateClientOrderID, ErrDuplicateClientMoveID, ErrDuplicateExternalID
/// </remarks>
[Serializable]
public class LunoIdempotencyException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoIdempotencyException"/> class.
    /// </summary>
    public LunoIdempotencyException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoIdempotencyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoIdempotencyException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoIdempotencyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoIdempotencyException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoIdempotencyException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoIdempotencyException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
