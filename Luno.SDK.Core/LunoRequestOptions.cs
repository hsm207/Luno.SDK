namespace Luno.SDK;

/// <summary>
/// Provides advanced configuration options for a Luno SDK request.
/// </summary>
public record LunoRequestOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether API credentials should be attached to this specific public request.
    /// When true, credentials are included for better rate limits.
    /// Default is false (Privacy/Security mode).
    /// </summary>
    public bool AuthenticatePublicEndpoint { get; init; }
 
    /// <summary>
    /// Gets or sets a value indicating whether the developer explicitly authorizes a write operation.
    /// Mandatory for all POST/PUT/DELETE requests identified as write operations.
    /// Default is false (Fail-Safe).
    /// </summary>
    public bool AuthorizeWriteOperation { get; init; }
}
