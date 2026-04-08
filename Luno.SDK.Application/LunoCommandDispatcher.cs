using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Application;

/// <summary>
/// The default implementation of a command dispatcher. 
/// It uses a resolver to find handlers and behaviors, composing them into an execution pipeline.
/// </summary>
/// <param name="resolver">A function that can resolve requested types from a composition root (like a DI container).</param>
public class LunoCommandDispatcher(Func<Type, object?> resolver) : ILunoCommandDispatcher
{
    /// <inheritdoc />
    public Task<TResponse> DispatchAsync<TRequest, TResponse>(TRequest request, LunoRequestOptions? options = null, CancellationToken ct = default)
    {
        using var scope = LunoSecurityContext.Set(options);

        // 1. Resolve the primary handler
        var handlerType = typeof(ICommandHandler<TRequest, TResponse>);
        var handler = (ICommandHandler<TRequest, TResponse>?)resolver(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for {handlerType.Name}");
        }

        // 2. Resolve all applicable pipeline behaviors
        var behaviorType = typeof(IEnumerable<IPipelineBehavior<TRequest, TResponse>>);
        var behaviors = (IEnumerable<IPipelineBehavior<TRequest, TResponse>>?)resolver(behaviorType) ?? [];

        // 3. Build the pipeline chain
        RequestHandlerDelegate<TResponse> pipelineChain = () => handler.HandleAsync(request, ct);

        // We reverse the behaviors so that the first registered one becomes the outermost wrapper
        foreach (var behavior in behaviors.Reverse())
        {
            var next = pipelineChain;
            pipelineChain = () => behavior.Handle(request, next, ct);
        }

        return pipelineChain();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStreamAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
    {
        // 1. Resolve the primary stream handler
        var handlerType = typeof(IStreamCommandHandler<TRequest, TResponse>);
        var handler = (IStreamCommandHandler<TRequest, TResponse>?)resolver(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No stream handler registered for {handlerType.Name}");
        }

        // 2. Resolve all applicable stream pipeline behaviors
        var behaviorType = typeof(IEnumerable<IStreamPipelineBehavior<TRequest, TResponse>>);
        var behaviors = (IEnumerable<IStreamPipelineBehavior<TRequest, TResponse>>?)resolver(behaviorType) ?? [];

        // 3. Build the pipeline chain
        StreamHandlerDelegate<TResponse> pipelineChain = () => handler.HandleAsync(request, ct);

        foreach (var behavior in behaviors.Reverse())
        {
            var next = pipelineChain;
            pipelineChain = () => behavior.Handle(request, next, ct);
        }

        return pipelineChain();
    }
}
