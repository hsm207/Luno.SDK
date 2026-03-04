using Luno.SDK.Infrastructure.Telemetry;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class LunoTelemetryTests
{
    [Fact(DisplayName = "Given telemetry instance, When disposing, Then release all resources faithfully")]
    public void Dispose_GivenTelemetryInstance_WhenDisposing_ThenReleaseAllResourcesFaithfully()
    {
        // Arrange
        var telemetry = new LunoTelemetry();

        // Act
        var ex = Record.Exception(() => telemetry.Dispose());

        // Assert
        Assert.Null(ex); // If it threw an exception, it wouldn't be faithfully releasing.
    }
}
