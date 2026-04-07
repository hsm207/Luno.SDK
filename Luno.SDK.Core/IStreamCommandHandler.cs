using System.Collections.Generic;
using System.Threading;

namespace Luno.SDK;

/// <summary>
/// Defines a handler for a specific command/response pair that returns a stream of results.
/// </summary>
/// <typeparam name="TRequest">The type of the command/request.</typeparam>
/// <typeparam name="TResponse">The type of the underlying response result in the stream.</typeparam>
public interface IStreamCommandHandler<in TRequest, TResponse>
{
    /// <summary>
    /// Handles the command asynchronously and returns a stream of results.
    /// </summary>
    /// <param name="request">The command object.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An asynchronous stream of response results.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}
