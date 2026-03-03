// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Luno.SDK.Infrastructure.Telemetry;

/// <summary>
/// Unified telemetry provider for the Luno SDK! 🏛️💎
/// Follows the official Microsoft & OpenTelemetry Gold Standard! 🏆✨
/// </summary>
public sealed class LunoTelemetry : IDisposable
{
    public const string Name = "Luno.SDK";
    public const string Version = "1.0.0";

    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }

    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _durationHistogram;

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

    public void RecordRequest(string operation, string status)
    {
        _requestCounter.Add(1, 
            new KeyValuePair<string, object?>("luno.operation", operation),
            new KeyValuePair<string, object?>("luno.status", status));
    }

    public void RecordDuration(double duration, string operation)
    {
        _durationHistogram.Record(duration, 
            new KeyValuePair<string, object?>("luno.operation", operation));
    }

    public void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }
}
