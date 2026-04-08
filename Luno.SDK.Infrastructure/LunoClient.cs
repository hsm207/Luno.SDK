using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions;
using Luno.SDK.Account;
using Luno.SDK.Infrastructure.Account;
using Luno.SDK.Infrastructure.Authentication;
using Luno.SDK.Infrastructure.ErrorHandling;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Telemetry;
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
    private readonly ILunoTelemetry _telemetry;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using standalone defaults.
    /// </summary>
    /// <param name="options">Optional configuration options for the client.</param>
    public LunoClient(LunoClientOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddLunoClient(opt =>
        {
            if (options != null)
            {
                opt.ApiKeyId = options.ApiKeyId;
                opt.ApiKeySecret = options.ApiKeySecret;
                opt.BaseUrl = options.BaseUrl;
                opt.UserAgent = options.UserAgent;
                opt.LoggerFactory = options.LoggerFactory;
            }
        });

        // We resolve the fully-wired dependencies from the internal container
        var sp = services.BuildServiceProvider();
        _telemetry = sp.GetRequiredService<ILunoTelemetry>();
        var dispatcher = sp.GetRequiredService<ILunoRequestDispatcher>();

        // Setup the specialized sub-clients (lazy-resolved from the same container)
        _market = new Lazy<ILunoMarketClient>(() => sp.GetRequiredService<ILunoMarketClient>());
        _accounts = new Lazy<ILunoAccountClient>(() => sp.GetRequiredService<ILunoAccountClient>());
        _trading = new Lazy<ILunoTradingClient>(() => sp.GetRequiredService<ILunoTradingClient>());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoClient"/> class using the specified dependencies.
    /// This constructor is primarily used by the DI system via AddLunoClient.
    /// </summary>
    public LunoClient(
        LunoClientOptions options,
        ILunoTelemetry telemetry,
        ILunoRequestDispatcher dispatcher,
        ILunoMarketClient market,
        ILunoAccountClient accounts,
        ILunoTradingClient trading)
    {
        _telemetry = telemetry;
        
        // In hosted mode, we use the provided instances
        _market = new Lazy<ILunoMarketClient>(() => market);
        _accounts = new Lazy<ILunoAccountClient>(() => accounts);
        _trading = new Lazy<ILunoTradingClient>(() => trading);
    }
}
