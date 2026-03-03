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
    /// Gets or sets the API version to target. Defaults to "1".
    /// </summary>
    public string ApiVersion { get; set; } = "1";
    
    /// <summary>
    /// Gets or sets the logger factory used to create loggers for the SDK.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;
}
