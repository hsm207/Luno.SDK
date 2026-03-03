// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.DependencyInjection;
using Luno.SDK; // THE MISSING SLAY! 💅✨

namespace Luno.SDK.Infrastructure.Market;

public static class LunoMarketServiceExtensions
{
    /// <summary>
    /// Adds the Luno SDK to the service collection with high-energy modern defaults! 💅✨
    /// </summary>
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

        return builder;
    }
}
