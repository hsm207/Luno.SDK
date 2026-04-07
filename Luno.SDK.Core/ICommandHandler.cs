using System.Threading;

namespace Luno.SDK;

/// <summary>
/// Defines a handler for a specific command/response pair.
/// </summary>
/// <typeparam name="TRequest">The type of the command/request.</typeparam>
/// <typeparam name="TResponse">The type of the underlying response result.</typeparam>
public interface ICommandHandler<in TRequest, TResponse>
{
    /// <summary>
    /// Handles the command asynchronously.
    /// </summary>
    /// <param name="request">The command object.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the response result.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}
