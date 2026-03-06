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
        // We trust the compiler for null checks.
        var authOption = request.RequestOptions.OfType<LunoAuthenticationOption>().FirstOrDefault();

        // 1. Explicit Control (The Edge Case)
        if (authOption != null && !authOption.RequiresAuthentication)
        {
            // Explicitly opted out. Do NOT authenticate.
            return Task.CompletedTask;
        }

        bool isMandatoryPrivate = IsMandatoryPrivateEndpoint(request.URI.AbsolutePath);

        // If explicitly requested OR naturally a mandatory private endpoint
        if ((authOption != null && authOption.RequiresAuthentication) || isMandatoryPrivate)
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
            // 2. Auto-Optimized Public (or Anonymous)
            // If we have credentials, we attach them to get the rate limit optimization
            if (_preComputedAuthHeader != null && !request.Headers.ContainsKey("Authorization"))
            {
                request.Headers.Add("Authorization", _preComputedAuthHeader);
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsMandatoryPrivateEndpoint(string path)
    {
        if (string.IsNullOrEmpty(path)) return true; // Fail-secure

        // Public Allowlist Strategy (Secure-by-Design)
        // Only explicitly known market data endpoints are considered public.
        // Everything else requires authentication.
        bool isPublic = path.StartsWith("/api/1/ticker", StringComparison.OrdinalIgnoreCase) || // covers /ticker and /tickers
                        path.StartsWith("/api/1/orderbook", StringComparison.OrdinalIgnoreCase) || // covers /orderbook and /orderbook_top
                        path.Equals("/api/1/trades", StringComparison.OrdinalIgnoreCase) ||
                        path.Equals("/api/exchange/1/markets", StringComparison.OrdinalIgnoreCase);

        return !isPublic;
    }
}
