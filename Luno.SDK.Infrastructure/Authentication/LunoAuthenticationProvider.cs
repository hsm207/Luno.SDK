using System.Text;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Luno.SDK.Infrastructure.Authentication;

/// <summary>
/// Provides HTTP Basic Authentication for Luno API requests.
/// Pre-computes the Base64 header value to minimize allocations.
/// </summary>
public class LunoAuthenticationProvider : IAuthenticationProvider
{
    private readonly string? _preComputedAuthHeader;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="options">The client options containing the API Key ID and Secret.</param>
    public LunoAuthenticationProvider(LunoClientOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ApiKeyId) && !string.IsNullOrWhiteSpace(options.ApiKeySecret))
        {
            var bytes = Encoding.UTF8.GetBytes($"{options.ApiKeyId}:{options.ApiKeySecret}");
            var base64 = Convert.ToBase64String(bytes);
            _preComputedAuthHeader = $"Basic {base64}";
        }
    }

    /// <inheritdoc />
    public Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Check if the request explicitly requires authentication
        var authOption = request.RequestOptions.OfType<LunoAuthenticationOption>().FirstOrDefault();
        bool requiresAuth = authOption?.RequiresAuthentication ?? false;

        if (requiresAuth)
        {
            if (_preComputedAuthHeader == null)
            {
                throw new LunoAuthenticationException("This request requires authentication, but API keys were not provided in LunoClientOptions.");
            }

            if (!request.Headers.ContainsKey("Authorization"))
            {
                request.Headers.Add("Authorization", _preComputedAuthHeader);
            }
        }
        else
        {
            // If the request doesn't explicitly require authentication, but we have credentials, we can still attach them
            // The RFC states: "Additionally, by authenticating 'public' requests, developers can leverage their per-account rate limit buckets."
            // So we add them if they are present and no auth header is already set.
            if (_preComputedAuthHeader != null && !request.Headers.ContainsKey("Authorization"))
            {
                request.Headers.Add("Authorization", _preComputedAuthHeader);
            }
        }

        return Task.CompletedTask;
    }
}
