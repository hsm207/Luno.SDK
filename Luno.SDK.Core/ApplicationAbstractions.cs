using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK;

/// <summary>
/// Defines the contract for an object which can dispatch commands to their respective handlers.
/// This abstraction allows the SDK to resolve handlers at runtime without forcing a DI container
/// on the consumer, while still adhering to the Dependency Inversion Principle.
/// </summary>
public interface ILunoCommandDispatcher
{
    /// <summary>
    /// Dispatches a command/query to its registered handler.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command/request.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response. Can be Task{T} or IAsyncEnumerable{T}.</typeparam>
    /// <param name="request">The command object carrying the intent and parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The response directly from the handler.</returns>
    TResponse DispatchAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default);
}

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
