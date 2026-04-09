using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK;

/// <summary>
/// Represents a provider capable of supplying Luno API credentials.
/// </summary>
/// <remarks>
/// IMPORTANT: Implementations should avoid caching decrypted values longer than necessary.
/// Synchronous providers (like in-memory) should return <see cref="ValueTask.FromResult"/>.
/// </remarks>
public interface ILunoCredentialProvider
{
    /// <summary>
    /// Asynchronously retrieves the Luno API credentials.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{LunoCredentials}"/> representing the asynchronous operation.</returns>
    ValueTask<LunoCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the fundamental API keys required to authenticate with Luno.
/// </summary>
/// <param name="ApiKeyId">The API Key ID.</param>
/// <param name="ApiKeySecret">The API Key Secret.</param>
public readonly record struct LunoCredentials(string ApiKeyId, string ApiKeySecret);
