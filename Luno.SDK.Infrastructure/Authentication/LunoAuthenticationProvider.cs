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
        if (options is { ApiKeyId: var id, ApiKeySecret: var secret }
            && !string.IsNullOrWhiteSpace(id)
            && !string.IsNullOrWhiteSpace(secret))
        {
            var bytes = Encoding.UTF8.GetBytes($"{id}:{secret}");
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
        // 1. Determine if authentication is required by Spec or User
        var requiredPermission = LunoSecurityMetadata.GetRequiredPermission(
            request.HttpMethod.ToString(),
            request.UrlTemplate ?? string.Empty);

        var authOption = request.RequestOptions.OfType<LunoAuthenticationOption>().FirstOrDefault();
        
        bool isMandatory = requiredPermission != null;
        bool isPublicOptIn = !isMandatory && authOption is { AuthenticatePublicEndpoints: true };
        bool shouldAuth = isMandatory || isPublicOptIn;

        // 2. Skip if not required and not requested
        if (!shouldAuth)
        {
            return Task.CompletedTask;
        }

        // 3. Early return if the header is already present
        if (request.Headers.ContainsKey("Authorization"))
        {
            return Task.CompletedTask;
        }

        // 4. Guard Clause: We must have keys if we are supposed to auth
        if (_preComputedAuthHeader == null)
        {
            var reason = isMandatory ? $"Mandatory Permission Required: {requiredPermission}" : "Explicitly Requested by User";
            throw new LunoAuthenticationException(
                $"This request ({request.HttpMethod} {request.UrlTemplate}) requires authentication ({reason}), but API keys were not provided.");
        }

        // 5. Attach Header
        request.Headers.TryAdd("Authorization", _preComputedAuthHeader);

        return Task.CompletedTask;
    }
}


