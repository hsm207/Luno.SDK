using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when a post-only order would trade immediately and is therefore cancelled.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrPostOnlyMode, ErrPostOnlyNotAllowed, ErrPostOnly
/// </remarks>
[Serializable]
public class LunoPostOnlyViolationException : LunoBusinessRuleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoPostOnlyViolationException"/> class.
    /// </summary>
    public LunoPostOnlyViolationException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoPostOnlyViolationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoPostOnlyViolationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoPostOnlyViolationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoPostOnlyViolationException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoPostOnlyViolationException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoPostOnlyViolationException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
