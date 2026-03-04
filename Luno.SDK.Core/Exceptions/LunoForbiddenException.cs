using System;
using System.Diagnostics.CodeAnalysis;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when the API returns a 403 Forbidden status code, indicating lack of permissions.
/// </summary>
public class LunoForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoForbiddenException"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public LunoForbiddenException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoForbiddenException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    [ExcludeFromCodeCoverage]
    public LunoForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}
