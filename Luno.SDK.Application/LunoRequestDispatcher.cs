using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Application;

/// <summary>
/// The default implementation of a request dispatcher. 
/// It uses a resolver to find handlers and behaviors, composing them into an execution pipeline.
/// </summary>
/// <param name="resolver">A function that can resolve requested types from a composition root (like a DI container).</param>
public class LunoRequestDispatcher(Func<Type, object?> resolver) : ILunoRequestDispatcher
{
    /// <inheritdoc />
    public Task<TResponse> SendAsync<TResponse>(ILunoRequest<TResponse> request, CancellationToken ct = default)
    {
        using var scope = LunoSecurityContext.Set(request.Options);

        var requestType = request.GetType();

        // 1. Resolve the primary handler
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = (ICommandHandlerBase<TResponse>?)resolver(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for {handlerType.Name}");
        }

        // 2. Resolve all applicable pipeline behaviors
        var behaviorType = typeof(IEnumerable<>).MakeGenericType(
            typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)));
        var behaviors = (IEnumerable<IPipelineBehaviorBase<TResponse>>?)resolver(behaviorType) ?? [];

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
    public IAsyncEnumerable<TResponse> CreateStreamAsync<TResponse>(ILunoRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();

        // 1. Resolve the primary stream handler
        var handlerType = typeof(IStreamCommandHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = (IStreamCommandHandlerBase<TResponse>?)resolver(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No stream handler registered for {handlerType.Name}");
        }

        // 2. Resolve all applicable stream pipeline behaviors
        var behaviorType = typeof(IEnumerable<>).MakeGenericType(
            typeof(IStreamPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)));
        var behaviors = (IEnumerable<IStreamPipelineBehaviorBase<TResponse>>?)resolver(behaviorType) ?? [];

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
