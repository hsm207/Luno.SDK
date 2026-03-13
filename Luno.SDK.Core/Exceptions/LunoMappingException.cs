using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when the API response data is incomplete or invalid and cannot be mapped to a domain entity.
/// </summary>
/// <remarks>
/// This exception represents parsing/data-binding failures not explicitly returned as error codes from the Luno API.
/// </remarks>
[Serializable]
public class LunoMappingException : LunoDataException
{
    /// <summary>
    /// Gets the name of the DTO type that failed to map.
    /// </summary>
    public string? DtoType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMappingException"/> class.
    /// </summary>
    public LunoMappingException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMappingException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoMappingException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMappingException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoMappingException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMappingException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoMappingException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMappingException"/> class with a specific DTO type name.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="dtoType">The name of the DTO type.</param>
    /// <param name="innerException">The inner exception (if any).</param>
    public LunoMappingException(string message, string? dtoType, Exception? innerException = null) : base(message, innerException)
    {
        DtoType = dtoType;
    }
}
