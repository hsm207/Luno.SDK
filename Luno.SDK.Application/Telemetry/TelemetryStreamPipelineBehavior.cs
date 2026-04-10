using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Telemetry;

namespace Luno.SDK.Application.Telemetry;

/// <summary>
/// A specialized pipeline behavior that records telemetry (latency, failures, and traces) for streaming command handlers.
/// This uses a manual iterator pattern to accurately track the duration from start to final yield, including any internal failures.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the underlying response result in the stream.</typeparam>
public class TelemetryStreamPipelineBehavior<TRequest, TResponse>(ILunoTelemetry telemetry) : IStreamPipelineBehavior<TRequest, TResponse>
{
    private static readonly string RequestName = typeof(TRequest).Name.ToLowerInvariant();

    private readonly ILunoTelemetry _telemetry = telemetry;
    private readonly Histogram<long> _latencyHistogram = telemetry.Meter.CreateHistogram<long>($"luna_stream_handler_{RequestName}_latency_ms");
    private readonly Counter<long> _failureCounter = telemetry.Meter.CreateCounter<long>($"luno_stream_handler_{RequestName}_failures");

    /// <inheritdoc />
    public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken ct)
    {
        var activity = _telemetry.ActivitySource.StartActivity(RequestName);
        activity?.SetTag("luno.operation", RequestName);
        activity?.SetTag("luno.stream", "true");

        var sw = Stopwatch.StartNew();

        var source = next();
        var enumerator = source.GetAsyncEnumerator(ct);

        try
        {
            bool hasNext;
            do
            {
                try
                {
                    hasNext = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    RecordFailure(ex, sw, activity);
                    throw;
                }

                if (hasNext)
                {
                    yield return enumerator.Current;
                }
            } while (hasNext);

            RecordSuccess(sw, activity);
        }
        finally
        {
            await enumerator.DisposeAsync();
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
