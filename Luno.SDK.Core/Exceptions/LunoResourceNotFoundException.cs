using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrNotFound, ErrAccountNotFound, ErrBeneficiaryNotFound, etc.
/// </remarks>
[Serializable]
public class LunoResourceNotFoundException : LunoBusinessRuleException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoResourceNotFoundException"/> class.
    /// </summary>
    public LunoResourceNotFoundException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoResourceNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoResourceNotFoundException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoResourceNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoResourceNotFoundException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoResourceNotFoundException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoResourceNotFoundException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
