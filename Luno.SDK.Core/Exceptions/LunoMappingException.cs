using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when the API response data is incomplete or invalid and cannot be mapped to a domain entity.
/// </summary>
public class LunoMappingException : LunoDataException
{
    /// <summary>
    /// Gets the type of the DTO that failed to map.
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
    public LunoMappingException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoMappingException"/> class with a specific DTO type.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="dtoType">The type of the DTO.</param>
    public LunoMappingException(string message, string dtoType) : base(message)
    {
        DtoType = dtoType;
    }
}
