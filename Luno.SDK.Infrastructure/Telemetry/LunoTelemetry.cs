using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Luno.SDK.Infrastructure.Telemetry;

/// <summary>
/// Provides unified telemetry for the Luno SDK using OpenTelemetry standards for tracing and metrics.
/// </summary>
public sealed class LunoTelemetry : IDisposable
{
    /// <summary>
    /// The name of the instrumentation source.
    /// </summary>
    public const string Name = "Luno.SDK";

    /// <summary>
    /// The version of the instrumentation source.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// Gets the <see cref="ActivitySource"/> for tracing SDK operations.
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// Gets the <see cref="Meter"/> for recording SDK metrics.
    /// </summary>
    public Meter Meter { get; }

    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _durationHistogram;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoTelemetry"/> class.
    /// </summary>
    public LunoTelemetry()
    {
        ActivitySource = new ActivitySource(Name, Version);
        Meter = new Meter(Name, Version);

        _requestCounter = Meter.CreateCounter<long>(
            "luno.sdk.requests", 
            unit: "{requests}", 
            description: "Total number of API requests made to Luno.");

        _durationHistogram = Meter.CreateHistogram<double>(
            "luno.sdk.request.duration", 
            unit: "ms", 
            description: "Latency of API requests made to Luno.");
    }

    /// <summary>
    /// Records an API request with the specified operation and status.
    /// </summary>
    /// <param name="operation">The name of the operation (e.g., GetTickers).</param>
    /// <param name="status">The result status (e.g., Success, Error).</param>
    public void RecordRequest(string operation, string status)
    {
        _requestCounter.Add(1, 
            new KeyValuePair<string, object?>("luno.operation", operation),
            new KeyValuePair<string, object?>("luno.status", status));
    }

    /// <summary>
    /// Records the duration of an API request.
    /// </summary>
    /// <param name="duration">The duration in milliseconds.</param>
    /// <param name="operation">The name of the operation.</param>
    public void RecordDuration(double duration, string operation)
    {
        _durationHistogram.Record(duration, 
            new KeyValuePair<string, object?>("luno.operation", operation));
    }

    /// <summary>
    /// Disposes the underlying <see cref="ActivitySource"/> and <see cref="Meter"/>.
    /// </summary>
    public void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }
}
