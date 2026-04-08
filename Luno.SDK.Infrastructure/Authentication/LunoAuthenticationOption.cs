using Microsoft.Kiota.Abstractions;

namespace Luno.SDK.Infrastructure.Authentication;

/// <summary>
/// Provides configuration options for authentication on a per-request basis.
/// </summary>
public class LunoAuthenticationOption : IRequestOption
{
    /// <summary>
    /// Gets or sets a value indicating whether API keys should be sent to public endpoints.
    /// When true, credentials are included even for endpoints that don't require authentication,
    /// which Luno rewards with higher rate limits.
    /// Private endpoints always authenticate regardless of this setting.
    /// Default is false (Least Privilege).
    /// </summary>
    public bool AuthenticatePublicEndpoints { get; set; }
}
