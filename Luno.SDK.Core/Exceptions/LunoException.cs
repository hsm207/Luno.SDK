using System;

namespace Luno.SDK;

/// <summary>
/// The root exception for all custom Luno SDK exceptions.
/// </summary>
/// <remarks>
/// This is the base exception class from which all other custom exceptions in the Luno SDK derive.
/// </remarks>
[Serializable]
public class LunoException : Exception
{
    /// <summary>
    /// The raw error code string returned by the Luno API, if applicable.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// The HTTP status code returned by the API, if applicable.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoException"/> class.
    /// </summary>
    public LunoException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
