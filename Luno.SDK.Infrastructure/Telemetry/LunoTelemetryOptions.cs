using Microsoft.Kiota.Abstractions;

namespace Luno.SDK.Infrastructure.Telemetry;

/// <summary>
/// Provides options for tracking telemetry for a specific Luno API request.
/// </summary>
internal class LunoTelemetryOptions(string operationName) : IRequestOption
{
    /// <summary>
    /// Gets the name of the operation for telemetry tracking.
    /// </summary>
    public string OperationName { get; } = operationName;
}
