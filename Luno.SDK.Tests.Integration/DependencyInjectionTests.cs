// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Microsoft.Extensions.DependencyInjection;
using Luno.SDK;
using Luno.SDK.Infrastructure.Market; // Updated! 🤌
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class DependencyInjectionTests
{
    [Fact(DisplayName = "AddLunoClient should register ILunoClient and resolve high-energy sub-clients! 🏛️💎")]
    public void AddLunoClient_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLunoClient(options =>
        {
            options.ApiVersion = "1";
        });
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<ILunoClient>();

        // Assert
        Assert.NotNull(client);
        Assert.IsType<LunoClient>(client);
        
        Assert.NotNull(client.Market);
        Assert.NotNull(client.GetMarketClient());
    }
}
