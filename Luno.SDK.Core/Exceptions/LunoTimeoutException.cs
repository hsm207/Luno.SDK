using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when a request times out (e.g. Deadline Exceeded).
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrDeadlineExceeded
/// </remarks>
[Serializable]
public class LunoTimeoutException : LunoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoTimeoutException"/> class.
    /// </summary>
    public LunoTimeoutException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoTimeoutException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}
