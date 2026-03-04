using System;

namespace Luno.SDK;

/// <summary>
/// The abstract root exception for all custom Luno SDK exceptions.
/// Allows developers to catch all SDK-specific errors in a single block.
/// </summary>
public abstract class LunoException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoException"/> class.
    /// </summary>
    protected LunoException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected LunoException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoException(string message, Exception innerException) : base(message, innerException) { }
}
