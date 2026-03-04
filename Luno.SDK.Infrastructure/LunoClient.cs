using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions.Authentication;
using Luno.SDK.Infrastructure.Telemetry;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoClient"/> interface.
/// Orchestrates specialized sub-clients and manages common infrastructure like telemetry.
/// </summary>
public class LunoClient : ILunoClient
{
    private static readonly HttpClient SharedHttpClient = CreateDefaultHttpClient();

    private readonly LunoClientOptions _options;
    private readonly LunoTelemetry _telemetry;
    private readonly Lazy<ILunoMarketClient> _market;

    /// <inheritdoc />
    public ILunoMarketClient Market => _market.Value;

    /// <inheritdoc />
    public ILunoTelemetry Telemetry => _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using standalone defaults.
    /// </summary>
    /// <param name="options">Optional configuration options for the client.</param>
    public LunoClient(LunoClientOptions? options = null)
        : this(SharedHttpClient, options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using the specified HTTP client and options.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests. Lifetime must be managed by the caller.</param>
    /// <param name="options">Optional configuration options for the client.</param>
    public LunoClient(HttpClient httpClient, LunoClientOptions? options = null)
    {
        _options = options ?? new LunoClientOptions();
        _telemetry = new LunoTelemetry();
        
        // Setup the high-fidelity request adapter with telemetry decoration
        var auth = new AnonymousAuthenticationProvider();
        var baseAdapter = new HttpClientRequestAdapter(auth, httpClient: httpClient);
        var decoratedAdapter = new LunoTelemetryAdapter(baseAdapter, _telemetry, _options.LoggerFactory.CreateLogger<LunoTelemetryAdapter>());

        _market = new Lazy<ILunoMarketClient>(() => 
            new LunoMarketClient(decoratedAdapter));
    }

    private static HttpClient CreateDefaultHttpClient()
    {
        var handler = new SocketsHttpHandler 
        { 
            PooledConnectionLifetime = TimeSpan.FromMinutes(2) 
        };

        return new HttpClient(handler, disposeHandler: true) 
        { 
            BaseAddress = new Uri("https://api.luno.com") 
        };
    }
}
