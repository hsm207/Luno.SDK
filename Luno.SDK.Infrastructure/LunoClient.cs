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
using Luno.SDK.Trading;
using Luno.SDK.Infrastructure.Trading;
using Luno.SDK.Application;
using Luno.SDK.Application.Trading;
using Luno.SDK.Application.Account;
using Luno.SDK.Application.Market;
using Luno.SDK.Infrastructure.Generated;

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
    private readonly LunoApiClient _apiClient;
    private readonly ILunoCommandDispatcher _dispatcher;
    private readonly Lazy<ILunoMarketClient> _market;
    private readonly Lazy<ILunoAccountClient> _accounts;
    private readonly Lazy<ILunoTradingClient> _trading;

    /// <inheritdoc />
    public ILunoMarketClient Market => _market.Value;

    /// <inheritdoc />
    public ILunoAccountClient Accounts => _accounts.Value;

    /// <inheritdoc />
    public ILunoTradingClient Trading => _trading.Value;

    /// <inheritdoc />
    public ILunoTelemetry Telemetry => _telemetry;

    /// <inheritdoc />
    public ILunoCommandDispatcher Commands => _dispatcher;

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

        _apiClient = new LunoApiClient(_requestAdapter);

        // 5. Setup the Command Dispatcher (The Application Orchestration Layer)
        var factory = new DefaultCommandHandlerFactory(
            new LazyTradingProxy(this),
            new LazyAccountProxy(this),
            new LazyMarketProxy(this),
            _options.CommandHandlerDecorator);

        _dispatcher = new LunoCommandDispatcher(factory.CreateHandler);

        // 6. Setup the specialized sub-clients
        _market = new Lazy<ILunoMarketClient>(() => new LunoMarketClient(_apiClient, _dispatcher));
        _accounts = new Lazy<ILunoAccountClient>(() => new LunoAccountClient(_apiClient, _dispatcher));
        _trading = new Lazy<ILunoTradingClient>(() => new LunoTradingClient(_apiClient, _dispatcher));
    }

    // ── Lazy Proxies to break circular dependency during factory initialization ──────────────────
    
    private class LazyTradingProxy(LunoClient parent) : ILunoTradingClient {
        public ILunoCommandDispatcher Commands => parent.Trading.Commands;
        public Task<OrderReference> FetchPostLimitOrderAsync(LimitOrderRequest request, CancellationToken ct = default) => parent.Trading.FetchPostLimitOrderAsync(request, ct);
        public Task<bool> FetchStopOrderAsync(string orderId, CancellationToken ct = default) => parent.Trading.FetchStopOrderAsync(orderId, ct);
        public Task<Order> FetchOrderAsync(string? orderId = null, string? clientOrderId = null, CancellationToken ct = default) => parent.Trading.FetchOrderAsync(orderId, clientOrderId, ct);
    }

    private class LazyAccountProxy(LunoClient parent) : ILunoAccountClient {
        public ILunoCommandDispatcher Commands => parent.Accounts.Commands;
        public Task<IReadOnlyList<Balance>> FetchBalancesAsync(CancellationToken ct = default) => parent.Accounts.FetchBalancesAsync(ct);
    }

    private class LazyMarketProxy(LunoClient parent) : ILunoMarketClient {
        public ILunoCommandDispatcher Commands => parent.Market.Commands;
        public IAsyncEnumerable<Ticker> FetchTickersAsync(CancellationToken ct = default) => parent.Market.FetchTickersAsync(ct);
        public Task<Ticker> FetchTickerAsync(string pair, CancellationToken ct = default) => parent.Market.FetchTickerAsync(pair, ct);
    }

    private static HttpClient CreatePooledClient(LunoClientOptions options)
    {
        return new HttpClient(SharedHandler, disposeHandler: false)
        {
            BaseAddress = new Uri(options.BaseUrl)
        };
    }
}
