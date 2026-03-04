using Microsoft.Kiota.Abstractions;

namespace Luno.SDK.Infrastructure.Authentication;

/// <summary>
/// Provides configuration options for authentication on a per-request basis.
/// </summary>
public class LunoAuthenticationOption : IRequestOption
{
    /// <summary>
    /// Gets or sets a value indicating whether the request requires authentication.
    /// </summary>
    public bool RequiresAuthentication { get; set; }
}
