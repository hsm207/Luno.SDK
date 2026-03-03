using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Infrastructure.Market;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoClient"/> interface.
/// Orchestrates specialized sub-clients and manages common infrastructure like HTTP and telemetry.
/// </summary>
public class LunoClient : ILunoClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LunoTelemetry _telemetry;
    private readonly bool _disposeClient;
    private readonly ILogger<LunoClient> _logger;
    private readonly LunoClientOptions _options;

    /// <inheritdoc />
    public ILunoMarketClient Market => GetMarketClient();

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using the specified options.
    /// </summary>
    /// <param name="options">The configuration options for the client.</param>
    public LunoClient(LunoClientOptions? options = null)
    {
        _options = options ?? new LunoClientOptions();
        _logger = _options.LoggerFactory.CreateLogger<LunoClient>();
        
        _telemetry = new LunoTelemetry();
        
        var handler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);
        
        _disposeClient = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using an existing <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="logger">An optional logger instance.</param>
    public LunoClient(HttpClient httpClient, ILogger<LunoClient>? logger = null)
    {
        _httpClient = httpClient;
        _options = new LunoClientOptions();
        _telemetry = new LunoTelemetry();
        _disposeClient = false;
        _logger = logger ?? NullLogger<LunoClient>.Instance;
    }

    /// <inheritdoc />
    public ILunoMarketClient GetMarketClient() => 
        new LunoMarketClient(_httpClient, _telemetry, _logger, _options.ApiVersion);

    /// <summary>
    /// Disposes the underlying HTTP client and telemetry resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposeClient) _httpClient.Dispose();
        _telemetry.Dispose();
        GC.SuppressFinalize(this);
    }
}
