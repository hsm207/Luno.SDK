using Microsoft.Extensions.DependencyInjection;
using Luno.SDK;

namespace Luno.SDK.Infrastructure.Market;

/// <summary>
/// Provides extension methods for registering Luno SDK services in a Dependency Injection container.
/// </summary>
public static class LunoMarketServiceExtensions
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

        var builder = services.AddHttpClient<ILunoClient, LunoClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        });

        builder.AddStandardResilienceHandler();

        services.AddTransient(sp => sp.GetRequiredService<ILunoClient>().Market);

        return builder;
    }
}
