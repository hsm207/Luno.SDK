using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Core.Authentication;

/// <summary>
/// A simple, in-memory credential provider for Luno API keys.
/// </summary>
/// <remarks>
/// This provider holds the API Key ID and Secret as managed strings in memory. 
/// While convenient for rapid development, it is the least memory-hardened posture,
/// as the plain text strings remain eligible for process memory dumps.
/// </remarks>
public class BasicInMemoryCredentialProvider : ILunoCredentialProvider
{
    private readonly string _apiKeyId;
    private readonly string _apiKeySecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicInMemoryCredentialProvider"/> class.
    /// </summary>
    /// <param name="apiKeyId">The API Key ID.</param>
    /// <param name="apiKeySecret">The API Key Secret.</param>
    public BasicInMemoryCredentialProvider(string apiKeyId, string apiKeySecret)
    {
        _apiKeyId = apiKeyId;
        _apiKeySecret = apiKeySecret;
    }

    /// <inheritdoc />
    public ValueTask<LunoCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(new LunoCredentials(_apiKeyId, _apiKeySecret));
    }
}
