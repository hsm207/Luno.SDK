using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Telemetry;

namespace Luno.SDK.Application.Telemetry;

/// <summary>
/// A generic pipeline behavior that records telemetry (latency, failures, and traces) for standard task-based command handlers.
/// This centralizes cross-cutting concerns, ensuring 100% observability across the application layer with Native AOT compatibility.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the underlying response result.</typeparam>
public class TelemetryPipelineBehavior<TRequest, TResponse>(ILunoTelemetry telemetry) : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly string RequestName = typeof(TRequest).Name.ToLowerInvariant();

    private readonly ILunoTelemetry _telemetry = telemetry;
    private readonly Histogram<long> _latencyHistogram = telemetry.Meter.CreateHistogram<long>($"luno_handler_{RequestName}_latency_ms");
    private readonly Counter<long> _failureCounter = telemetry.Meter.CreateCounter<long>($"luno_handler_{RequestName}_failures");

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var activity = _telemetry.ActivitySource.StartActivity(RequestName);
        activity?.SetTag("luno.operation", RequestName);

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next();
            
            RecordSuccess(sw, activity);
            return response;
        }
        catch (Exception ex)
        {
            RecordFailure(ex, sw, activity);
            throw;
        }
    }

    private void RecordSuccess(Stopwatch sw, Activity? activity)
    {
        sw.Stop();
        activity?.SetTag("luno.status", "Success");
        _latencyHistogram.Record(sw.ElapsedMilliseconds);
        activity?.Dispose();
    }

    private void RecordFailure(Exception ex, Stopwatch sw, Activity? activity)
    {
        sw.Stop();
        _failureCounter.Add(1);
        _latencyHistogram.Record(sw.ElapsedMilliseconds);

        if (activity != null)
        {
            activity.SetTag("luno.status", "Error");
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity.Dispose();
        }
    }
}
