using System.Threading;

namespace Luno.SDK;

/// <summary>
/// Defines a handler for a specific command/response pair.
/// </summary>
/// <typeparam name="TRequest">The type of the command/request.</typeparam>
/// <typeparam name="TResponse">The type of the response (e.g., Task{T} or IAsyncEnumerable{T}).</typeparam>
public interface ICommandHandler<in TRequest, out TResponse>
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="request">The command object.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The response (Task or IAsyncEnumerable).</returns>
    TResponse HandleAsync(TRequest request, CancellationToken ct = default);
}
