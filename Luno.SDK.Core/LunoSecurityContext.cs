using System.Threading;

namespace Luno.SDK;

/// <summary>
/// Provides a scoped context for Luno per-request options and security intent.
/// This uses <see cref="AsyncLocal{T}"/> to propagate intent across architectural boundaries.
/// </summary>
public static class LunoSecurityContext
{
    private static readonly AsyncLocal<LunoRequestOptions?> _currentOptions = new();

    /// <summary>
    /// Gets or sets the current <see cref="LunoRequestOptions"/> for the active asynchronous flow.
    /// </summary>
    public static LunoRequestOptions? Current
    {
        get => _currentOptions.Value;
        set => _currentOptions.Value = value;
    }

    /// <summary>
    /// Creates a scope with the provided options.
    /// </summary>
    /// <param name="options">The options to apply to this scope.</param>
    /// <returns>A scope that restores previous options on disposal.</returns>
    public static Scope Set(LunoRequestOptions? options)
    {
        return new Scope(options);
    }

    /// <summary>
    /// Represents a disposable scope for security options.
    /// </summary>
    public readonly struct Scope : System.IDisposable
    {
        private readonly LunoRequestOptions? _previous;

        internal Scope(LunoRequestOptions? options)
        {
            _previous = _currentOptions.Value;
            _currentOptions.Value = options;
        }

        /// <summary>
        /// Restores the previous security context options.
        /// </summary>
        public void Dispose()
        {
            _currentOptions.Value = _previous;
        }
    }
}
