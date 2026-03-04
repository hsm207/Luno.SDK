namespace Luno.SDK;

/// <summary>
/// Exception thrown when the API returns a 401 Unauthorized status code, indicating invalid credentials.
/// </summary>
public class LunoUnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoUnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoUnauthorizedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoUnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoUnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
}
