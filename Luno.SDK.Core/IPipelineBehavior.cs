using System.Threading.Tasks;

namespace Luno.SDK;

/// <summary>
/// A delegate representing the next step in the command pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the underlying response result.</typeparam>
/// <returns>A task representing the result of the next handler or behavior.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Defines a behavior in the command execution pipeline for task-based requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request command.</typeparam>
/// <typeparam name="TResponse">The type of the underlying response result.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Handles the behavior logic and calls the next step in the pipeline.
    /// </summary>
    /// <param name="request">The command request.</param>
    /// <param name="next">The delegate to call the next handler or behavior in the chain.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the response result.</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct);
}
