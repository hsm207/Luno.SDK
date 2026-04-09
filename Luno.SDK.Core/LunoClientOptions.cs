using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Luno.SDK;

/// <summary>
/// Provides configuration options for the Luno SDK client.
/// </summary>
public class LunoClientOptions
{
    /// <summary>
    /// Gets or sets the base URL for the Luno API. Defaults to "https://api.luno.com".
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.luno.com";

    /// <summary>
    /// Gets or sets the User-Agent string sent with each request.
    /// </summary>
    public string UserAgent { get; set; } = "Luno.SDK/1.0.0 (.NET 10)";

    /// <summary>
    /// Gets or sets the logger factory used to create loggers for the SDK.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

    /// <summary>
    /// Gets or sets the credential provider used for authenticating requests.
    /// </summary>
    public ILunoCredentialProvider? Credentials { get; set; }

    /// <summary>
    /// Configures the client to use simple in-memory credentials.
    /// Note: This is the least memory-hardened posture as the plain text strings will reside on the heap.
    /// </summary>
    /// <param name="apiKeyId">The API Key ID.</param>
    /// <param name="apiKeySecret">The API Key Secret.</param>
    /// <returns>The current options instance for fluent chaining.</returns>
    public LunoClientOptions WithCredentials(string apiKeyId, string apiKeySecret)
    {
        Credentials = new Luno.SDK.Core.Authentication.BasicInMemoryCredentialProvider(apiKeyId, apiKeySecret);
        return this;
    }
}
