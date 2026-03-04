using System;

namespace Luno.SDK;

/// <summary>
/// The base exception for all data-related errors (Mapping, Validation).
/// </summary>
public abstract class LunoDataException : LunoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoDataException"/> class.
    /// </summary>
    protected LunoDataException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected LunoDataException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoDataException(string message, Exception innerException) : base(message, innerException) { }
}
