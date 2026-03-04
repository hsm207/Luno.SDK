namespace Luno.SDK;

/// <summary>
/// Provides constant identifiers for Luno SDK instrumentation, used for OpenTelemetry configuration.
/// </summary>
public static class LunoInstrumentation
{
    /// <summary>
    /// The name of the instrumentation source for traces and metrics.
    /// </summary>
    public const string Name = "Luno.SDK";

    /// <summary>
    /// The current version of the instrumentation source.
    /// </summary>
    public const string Version = "1.0.0";
}
