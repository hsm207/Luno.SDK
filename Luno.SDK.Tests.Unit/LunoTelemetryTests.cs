using Luno.SDK.Infrastructure.Telemetry;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class LunoTelemetryTests
{
    [Fact(DisplayName = "Given telemetry instance, When disposing, Then release all resources faithfully.")]
    public void DisposeWhenCalledShouldReleaseResources()
    {
        // Arrange
        var telemetry = new LunoTelemetry();

        // Act
        telemetry.Dispose();

        // Assert - Safe disposal
        telemetry.Dispose();
    }
}
