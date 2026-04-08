using System.Collections.Generic;
using System.Threading;

namespace Luno.SDK;

/// <summary>
/// A delegate representing the next step in the streaming command pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the underlying response result in the stream.</typeparam>
/// <returns>An asynchronous stream of response results.</returns>
public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>();

/// <summary>
/// A non-generic base interface for stream pipeline behaviors to facilitate runtime dispatching.
/// </summary>
public interface IStreamPipelineBehaviorBase<TResponse>
{
    /// <summary>
    /// Handles the behavior logic for a request object and calls the next step in the streaming pipeline.
    /// </summary>
    IAsyncEnumerable<TResponse> Handle(object request, StreamHandlerDelegate<TResponse> next, CancellationToken ct);
}

/// <summary>
/// Defines a behavior in the command execution pipeline for streaming requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request command.</typeparam>
/// <typeparam name="TResponse">The type of the underlying response result in the stream.</typeparam>
public interface IStreamPipelineBehavior<in TRequest, TResponse> : IStreamPipelineBehaviorBase<TResponse>
{
    /// <summary>
    /// Handles the behavior logic and calls the next step in the streaming pipeline.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="next">The delegate to call the next stream handler or behavior in the chain.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An asynchronous stream of response results from the pipeline.</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken ct);

    /// <inheritdoc />
    IAsyncEnumerable<TResponse> IStreamPipelineBehaviorBase<TResponse>.Handle(object request, StreamHandlerDelegate<TResponse> next, CancellationToken ct)
        => Handle((TRequest)request, next, ct);
}
