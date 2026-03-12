using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// The base exception for all data-related errors (Mapping, Validation).
/// </summary>
[Serializable]
public class LunoDataException : LunoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoDataException"/> class.
    /// </summary>
    public LunoDataException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoDataException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoDataException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoDataException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoDataException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
