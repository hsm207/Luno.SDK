// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Luno.SDK;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoClientTests
{
    [Fact(DisplayName = "LunoClient standalone should initialize with pristine defaults 💅✨")]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        using var client = new LunoClient();

        // Assert
        Assert.NotNull(client.Market);
        var marketClient = client.GetMarketClient();
        Assert.NotNull(marketClient);
    }

    [Fact(DisplayName = "LunoClient should dispose internal resources properly 🧼🛑")]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var client = new LunoClient();

        // Act & Assert
        client.Dispose(); // Should not throw
    }
}
