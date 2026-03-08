using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions;
using Luno.SDK.Account;
using Luno.SDK.Infrastructure.Account;
using Luno.SDK.Infrastructure.Authentication;
using Luno.SDK.Infrastructure.ErrorHandling;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Market;
using Luno.SDK.Infrastructure.Market;

namespace Luno.SDK;

/// <summary>
/// Provides a concrete implementation of the <see cref="ILunoClient"/> interface.
/// Orchestrates specialized sub-clients and manages common infrastructure like telemetry.
/// </summary>
public class LunoClient : ILunoClient
{
    // High-performance process-wide connection pool
    private static readonly SocketsHttpHandler SharedHandler = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    };

    private readonly LunoClientOptions _options;
    private readonly ILunoTelemetry _telemetry;
    private readonly IRequestAdapter _requestAdapter;
    private readonly Lazy<ILunoMarketClient> _market;
    private readonly Lazy<ILunoAccountClient> _accounts;

    /// <inheritdoc />
    public ILunoMarketClient Market => _market.Value;

    /// <inheritdoc />
    public ILunoAccountClient Accounts => _accounts.Value;

    /// <inheritdoc />
    public ILunoTelemetry Telemetry => _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using standalone defaults.
    /// </summary>
    /// <param name="options">Optional configuration options for the client.</param>
    public LunoClient(LunoClientOptions? options = null)
        : this(CreatePooledClient(options ?? new LunoClientOptions()), options)
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

        var telemetryImpl = new LunoTelemetry();
        _telemetry = telemetryImpl;

        // Setup the centralized request adapter pipeline
        var authProvider = new LunoAuthenticationProvider(_options);
        var baseAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);

        var errorHandlingAdapter = new LunoErrorHandlingAdapter(baseAdapter);

        _requestAdapter = new LunoTelemetryAdapter(
            errorHandlingAdapter,
            telemetryImpl,
            _options.LoggerFactory.CreateLogger<LunoTelemetryAdapter>());

        _market = new Lazy<ILunoMarketClient>(() => new LunoMarketClient(_requestAdapter));
        _accounts = new Lazy<ILunoAccountClient>(() => new LunoAccountClient(_requestAdapter));
    }

    private static HttpClient CreatePooledClient(LunoClientOptions options)
    {
        return new HttpClient(SharedHandler, disposeHandler: false)
        {
            BaseAddress = new Uri(options.BaseUrl)
        };
    }
}
