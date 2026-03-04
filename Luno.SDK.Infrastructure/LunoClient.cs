using Microsoft.Extensions.Logging;
using Luno.SDK.Infrastructure.Telemetry;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoClient"/> interface.
/// Orchestrates specialized sub-clients and manages common infrastructure like telemetry.
/// </summary>
/// <param name="httpClient">The HTTP client to use for requests. Lifetime must be managed by the caller.</param>
/// <param name="options">Optional configuration options for the client.</param>
public class LunoClient(HttpClient httpClient, LunoClientOptions? options = null) : ILunoClient
{
    private static readonly HttpClient SharedHttpClient = CreateDefaultHttpClient();

    private readonly LunoClientOptions _options = options ?? new LunoClientOptions();
    private readonly LunoTelemetry _telemetry = new();
    private readonly Lazy<ILunoMarketClient> _market = new(() => 
        new LunoMarketClient(httpClient, new LunoTelemetry(), (options ?? new LunoClientOptions()).LoggerFactory.CreateLogger<LunoMarketClient>()));

    /// <inheritdoc />
    public ILunoMarketClient Market => _market.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using standalone defaults.
    /// </summary>
    /// <param name="options">Optional configuration options for the client.</param>
    public LunoClient(LunoClientOptions? options = null)
        : this(SharedHttpClient, options)
    {
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
