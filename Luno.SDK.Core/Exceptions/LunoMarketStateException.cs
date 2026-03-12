using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// Exception representing maintenance and restrictive market modes.
/// </summary>
[Serializable]
public class LunoMarketStateException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class.
    /// </summary>
    public LunoMarketStateException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoMarketStateException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoMarketStateException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMarketStateException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoMarketStateException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
