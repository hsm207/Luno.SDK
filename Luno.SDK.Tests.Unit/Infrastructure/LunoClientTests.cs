using System;
using Luno.SDK;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
public class LunoClientTests
{
    [Fact(DisplayName = "Given default constructor, When creating LunoClient, Then initialize with default options and pooled client.")]
    public void Constructor_Default_InitializesDefaultSharedClient()
    {
        // Act
        // This explicitly covers the `options ?? new LunoClientOptions()` fallback path.
        var client = new LunoClient();

        // Assert core components are wired
        Assert.NotNull(client.Market);
        Assert.NotNull(client.Accounts);
        Assert.NotNull(client.Telemetry);
    }
}
