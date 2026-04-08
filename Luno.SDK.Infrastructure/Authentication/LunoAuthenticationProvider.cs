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

        // Check for explicit intent attached to the request, or peek at the LunoSecurityContext
        var requestOptions = request.RequestOptions.OfType<LunoRequestOptions>().FirstOrDefault()
                            ?? LunoSecurityContext.Current;

        var authorizePublic = requestOptions?.AuthenticatePublicEndpoint ?? false;
        var authorizeWrite = requestOptions?.AuthorizeWriteOperation ?? false;

        // 2. Write Intent Sentry (Pre-flight guard)
        if (requiredPermission?.StartsWith("Perm_W") == true && !authorizeWrite)
        {
            throw new LunoSecurityException(
                request.HttpMethod.ToString(),
                request.UrlTemplate ?? string.Empty,
                requiredPermission);
        }

        bool isMandatory = requiredPermission != null;
        bool shouldAuth = isMandatory || authorizePublic;

        // 3. Skip if not required and not requested
        if (!shouldAuth)
        {
            return Task.CompletedTask;
        }

        // 4. Early return if the header is already present
        if (request.Headers.ContainsKey("Authorization"))
        {
            return Task.CompletedTask;
        }

        // 5. Guard Clause: We must have keys if we are supposed to auth
        if (_preComputedAuthHeader == null)
        {
            var reason = isMandatory ? $"Mandatory Permission Required: {requiredPermission}" : "Explicitly Requested by User";
            throw new LunoAuthenticationException(
                $"This request ({request.HttpMethod} {request.UrlTemplate}) requires authentication ({reason}), but API keys were not provided.");
        }

        // 6. Attach Header
        request.Headers.TryAdd("Authorization", _preComputedAuthHeader);

        return Task.CompletedTask;
    }
}


