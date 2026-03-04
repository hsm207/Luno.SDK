using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Infrastructure.Market;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoClient"/> interface.
/// Orchestrates specialized sub-clients and manages common infrastructure like telemetry.
/// </summary>
public class LunoClient : ILunoClient
{
    private readonly HttpClient _httpClient;
    private readonly LunoTelemetry _telemetry;
    private readonly ILogger<LunoClient> _logger;
    private readonly LunoClientOptions _options;

    /// <inheritdoc />
    public ILunoMarketClient Market => GetMarketClient();

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using the specified HTTP client and options.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests. Lifetime must be managed by the caller.</param>
    /// <param name="options">Optional configuration options for the client.</param>
    public LunoClient(HttpClient httpClient, LunoClientOptions? options = null)
    {
        _httpClient = httpClient;
        _options = options ?? new LunoClientOptions();
        _logger = _options.LoggerFactory.CreateLogger<LunoClient>();
        _telemetry = new LunoTelemetry();
    }

    /// <inheritdoc />
    public ILunoMarketClient GetMarketClient() => 
        new LunoMarketClient(_options, _httpClient);
}
