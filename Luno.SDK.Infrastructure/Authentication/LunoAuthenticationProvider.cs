using System.Security.Cryptography;
using System.Text;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Luno.SDK.Infrastructure.Authentication;

/// <summary>
/// Provides HTTP Basic Authentication for Luno API requests.
/// Implements late materialization of credentials to minimize memory-dump exposure.
/// </summary>
public class LunoAuthenticationProvider : IAuthenticationProvider
{
    private readonly ILunoCredentialProvider? _credentialProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAuthenticationProvider"/> class.
    /// </summary>
    /// <param name="options">The client options containing the credential provider.</param>
    public LunoAuthenticationProvider(LunoClientOptions options)
    {
        _credentialProvider = options.Credentials;
    }

    /// <inheritdoc />
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldAuthenticateRequest(request, out string? requiredPermission))
        {
            return;
        }

        var credentials = await FetchCredentialsAsync(request, requiredPermission, cancellationToken);
        AttachSecureHeader(request, credentials);
    }

    private bool ShouldAuthenticateRequest(RequestInformation request, out string? requiredPermission)
    {
        requiredPermission = LunoSecurityMetadata.GetRequiredPermission(
            request.HttpMethod.ToString(),
            request.UrlTemplate ?? string.Empty);

        var requestOptions = request.RequestOptions.OfType<LunoRequestOptions>().FirstOrDefault()
                            ?? LunoSecurityContext.Current;

        var authorizePublic = requestOptions?.AuthenticatePublicEndpoint ?? false;
        var authorizeWrite = requestOptions?.AuthorizeWriteOperation ?? false;

        if (requiredPermission?.StartsWith("Perm_W") == true && !authorizeWrite)
        {
            throw new LunoSecurityException(
                request.HttpMethod.ToString(),
                request.UrlTemplate ?? string.Empty,
                requiredPermission);
        }

        bool shouldAuth = (requiredPermission != null) || authorizePublic;
        bool alreadyAuthed = request.Headers.ContainsKey("Authorization");

        return shouldAuth && !alreadyAuthed;
    }

    private async Task<LunoCredentials> FetchCredentialsAsync(RequestInformation request, string? requiredPermission, CancellationToken cancellationToken)
    {
        if (_credentialProvider == null)
        {
            var reason = requiredPermission != null ? $"Mandatory Permission Required: {requiredPermission}" : "Explicitly Requested by User";
            throw new LunoAuthenticationException(
                $"This request ({request.HttpMethod} {request.UrlTemplate}) requires authentication ({reason}), but no ICredentialProvider was configured.");
        }

        var credentials = await _credentialProvider.GetCredentialsAsync(cancellationToken);
        
        if (string.IsNullOrWhiteSpace(credentials.ApiKeyId) || string.IsNullOrWhiteSpace(credentials.ApiKeySecret))
        {
            throw new LunoAuthenticationException("The configured credential provider returned empty keys.");
        }

        return credentials;
    }

    private void AttachSecureHeader(RequestInformation request, LunoCredentials credentials)
    {
        int len = credentials.ApiKeyId.Length + 1 + credentials.ApiKeySecret.Length;
        char[] charBuffer = System.Buffers.ArrayPool<char>.Shared.Rent(len);
        byte[] byteBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(len));
        
        try
        {
            credentials.ApiKeyId.CopyTo(0, charBuffer, 0, credentials.ApiKeyId.Length);
            charBuffer[credentials.ApiKeyId.Length] = ':';
            credentials.ApiKeySecret.CopyTo(0, charBuffer, credentials.ApiKeyId.Length + 1, credentials.ApiKeySecret.Length);

            int byteCount = Encoding.UTF8.GetBytes(charBuffer, 0, len, byteBuffer, 0);
            string base64 = Convert.ToBase64String(byteBuffer, 0, byteCount);

            request.Headers.TryAdd("Authorization", $"Basic {base64}");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(System.Runtime.InteropServices.MemoryMarshal.AsBytes(charBuffer.AsSpan()));
            CryptographicOperations.ZeroMemory(byteBuffer);
            System.Buffers.ArrayPool<char>.Shared.Return(charBuffer);
            System.Buffers.ArrayPool<byte>.Shared.Return(byteBuffer);
        }
    }
}


