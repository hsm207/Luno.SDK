using System.Collections.Generic;
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
    /// Dispatches a command/query to its registered handler for a single result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command/request.</typeparam>
    /// <typeparam name="TResponse">The type of the underlying response result.</typeparam>
    /// <param name="request">The command object.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task containing the response result.</returns>
    Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default);

    /// <summary>
    /// Dispatches a command/query to its registered handler for a stream of results.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command/request.</typeparam>
    /// <typeparam name="TResponse">The type of the results in the stream.</typeparam>
    /// <param name="request">The command object.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An asynchronous stream of response results.</returns>
    IAsyncEnumerable<TResponse> CreateStreamAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default);
}
