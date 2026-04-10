using System.Text;
using Microsoft.Extensions.Logging;

namespace Luno.SDK.Tests.Integration.Infrastructure;

/// <summary>
/// A shared test fixture for capturing and inspecting Loglevel-specific output in integration tests.
/// Useful for verifying security policies (redaction) and pipeline metadata.
/// </summary>
public class MemoryLoggerProvider(StringBuilder logSink) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new MemoryLogger(categoryName, logSink);
    public void Dispose() { }
}

public class MemoryLogger(string categoryName, StringBuilder logSink) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        lock (logSink)
        {
            logSink.AppendLine($"[{categoryName}] {message}");
        }
    }
}
