using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Luno.SDK.Application;
using Luno.SDK.Market;
using Luno.SDK.Account;
using Luno.SDK.Trading;
using Luno.SDK.Infrastructure.Market;
using Luno.SDK.Infrastructure.Account;
using Luno.SDK.Infrastructure.Trading;
using Luno.SDK.Infrastructure.Generated;
using Luno.SDK.Infrastructure.Authentication;
using Luno.SDK.Infrastructure.ErrorHandling;
using Luno.SDK.Infrastructure.Telemetry;
using Luno.SDK.Telemetry;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Luno.SDK;

/// <summary>
/// Provides extension methods for registering Luno SDK services in a Dependency Injection container.
/// </summary>
public static class LunoServiceExtensions
{
    /// <summary>
    /// Adds the Luno SDK client and related services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">An optional delegate to configure the <see cref="LunoClientOptions"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
    public static IHttpClientBuilder AddLunoClient(this IServiceCollection services, Action<LunoClientOptions>? configureOptions = null)
    {
        var options = new LunoClientOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);
        
        // 1. Core Abstractions & Telemetry
        services.TryAddSingleton<LunoTelemetry>();
        services.TryAddSingleton<ILunoTelemetry>(sp => sp.GetRequiredService<LunoTelemetry>());
        services.TryAddSingleton(sp => sp.GetRequiredService<LunoClientOptions>().LoggerFactory);

        // 2. Request Adapter Pipeline (Kiota)
        services.TryAddSingleton<IAuthenticationProvider, LunoAuthenticationProvider>();
        
        // Register the Kiota Request Adapter using IHttpClientFactory for proper lifecycle management
        services.AddHttpClient("Luno.SDK", (sp, client) =>
        {
            var spOptions = sp.GetRequiredService<LunoClientOptions>();
            client.BaseAddress = new Uri(spOptions.BaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", spOptions.UserAgent);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        })
        .AddStandardResilienceHandler();

        services.TryAddSingleton<IRequestAdapter>(sp => 
        {
            var auth = sp.GetRequiredService<IAuthenticationProvider>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<LunoTelemetryAdapter>();
            var telemetry = sp.GetRequiredService<LunoTelemetry>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            
            var httpClient = httpClientFactory.CreateClient("Luno.SDK");

            var baseAdapter = new HttpClientRequestAdapter(auth, httpClient: httpClient);
            var errorAdapter = new LunoErrorHandlingAdapter(baseAdapter);
            return new LunoTelemetryAdapter(errorAdapter, telemetry, logger);
        });

        // 3. API Client Wrapper
        services.TryAddSingleton<LunoApiClient>();

        // 4. Command Dispatcher & Handlers
        services.TryAddSingleton<ILunoCommandDispatcher>(sp => new LunoCommandDispatcher(type => sp.GetService(type)));
        RegisterCommandHandlers(services);

        // 5. Sub-Clients (Breaking circularity by registering implementation types)
        services.TryAddTransient<LunoMarketClient>();
        services.TryAddTransient<ILunoMarketClient>(sp => sp.GetRequiredService<LunoMarketClient>());
        services.TryAddTransient<ILunoMarketOperations>(sp => sp.GetRequiredService<LunoMarketClient>());

        services.TryAddTransient<LunoAccountClient>();
        services.TryAddTransient<ILunoAccountClient>(sp => sp.GetRequiredService<LunoAccountClient>());
        services.TryAddTransient<ILunoAccountOperations>(sp => sp.GetRequiredService<LunoAccountClient>());

        services.TryAddTransient<LunoTradingClient>();
        services.TryAddTransient<ILunoTradingClient>(sp => sp.GetRequiredService<LunoTradingClient>());
        services.TryAddTransient<ILunoTradingOperations>(sp => sp.GetRequiredService<LunoTradingClient>());

        // 6. Master Facade Client
        services.TryAddSingleton<ILunoClient, LunoClient>();

        return services.AddHttpClient("Luno.SDK"); // Return the builder for further config
    }

    /// <summary>
    /// Registers a pipeline behavior for all compatible commands.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="behaviorType">The open generic type of the behavior (e.g. typeof(LoggingBehavior&lt;,&gt;)).</param>
    public static IServiceCollection AddLunoCommandBehavior(this IServiceCollection services, Type behaviorType)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), behaviorType);
        return services;
    }

    private static void RegisterCommandHandlers(IServiceCollection services)
    {
        var handlerInterface = typeof(ICommandHandler<,>);
        var applicationAssembly = typeof(LunoCommandDispatcher).Assembly;

        var handlers = applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                .Select(i => new { Interface = i, Implementation = t }));

        foreach (var handler in handlers)
        {
            services.TryAddTransient(handler.Interface, handler.Implementation);
        }
    }
}
