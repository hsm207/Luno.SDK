using System;
using System.Runtime.Serialization;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when a client-side error occurs before a request is sent.
/// </summary>
[Serializable]
public class LunoClientException : LunoException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClientException"/> class.
    /// </summary>
    public LunoClientException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClientException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoClientException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClientException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoClientException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClientException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoClientException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
    }

