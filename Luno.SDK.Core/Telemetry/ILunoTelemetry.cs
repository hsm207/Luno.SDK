using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Luno.SDK.Telemetry;

/// <summary>
/// Defines the interface for accessing Luno SDK observability signals, including traces and metrics.
/// </summary>
public interface ILunoTelemetry
{
    /// <summary>
    /// Gets the <see cref="ActivitySource"/> used for tracing SDK operations.
    /// </summary>
    ActivitySource ActivitySource { get; }

    /// <summary>
    /// Gets the <see cref="Meter"/> used for recording SDK metrics.
    /// </summary>
    Meter Meter { get; }
}
