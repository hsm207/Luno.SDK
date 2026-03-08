using System;

namespace Luno.SDK;

/// <summary>
/// The base exception for all security-related errors (Authentication, Authorization).
/// </summary>
public abstract class LunoSecurityException : LunoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    protected LunoSecurityException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected LunoSecurityException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoSecurityException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoSecurityException(string message, Exception innerException) : base(message, innerException) { }
}
