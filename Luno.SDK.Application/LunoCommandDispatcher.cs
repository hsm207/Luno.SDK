using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Account;
using Luno.SDK.Application.Account;
using Luno.SDK.Application.Market;
using Luno.SDK.Application.Trading;
using Luno.SDK.Market;

namespace Luno.SDK.Application;

/// <summary>
/// The default implementation of a command dispatcher. 
/// It uses a factory to resolve handlers and allows for cross-cutting behaviors.
/// </summary>
public class LunoCommandDispatcher(Func<Type, object, object> handlerFactory) : ILunoCommandDispatcher
{
    /// <inheritdoc />
    public TResponse DispatchAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
    {
        // Resolve the handler for this specific request/response pair
        var handler = (ICommandHandler<TRequest, TResponse>)handlerFactory(typeof(ICommandHandler<TRequest, TResponse>), request!);
        
        return handler.HandleAsync(request, ct);
    }
}

/// <summary>
/// A factory that creates handlers for Luno commands.
/// This is where the magic happens: it maps Commands to Handlers and can wrap them in behaviors.
/// </summary>
public class DefaultCommandHandlerFactory(
    ILunoTradingClient trading,
    ILunoAccountClient account,
    ILunoMarketClient market,
    Func<object, object>? userDecorator = null)
{
    /// <summary>
    /// Creates a handler for the specified handler type and request.
    /// </summary>
    /// <param name="handlerType">The interface type of the handler.</param>
    /// <param name="request">The command object.</param>
    /// <returns>A concrete handler instance.</returns>
    public object CreateHandler(Type handlerType, object request)
    {
        // Trading
        if (handlerType == typeof(ICommandHandler<PostLimitOrderCommand, Task<OrderResponse>>))
            return Wrap(new PostLimitOrderHandler(trading));
        
        if (handlerType == typeof(ICommandHandler<StopOrderCommand, Task<OrderResponse>>))
            return Wrap(new StopOrderHandler(trading));

        // Account
        if (handlerType == typeof(ICommandHandler<GetBalancesQuery, Task<IReadOnlyList<AccountBalanceResponse>>>))
            return Wrap(new GetBalancesHandler(account));

        // Market
        if (handlerType == typeof(ICommandHandler<GetTickersQuery, IAsyncEnumerable<TickerResponse>>))
            return Wrap(new GetTickersHandler(market));

        if (handlerType == typeof(ICommandHandler<GetTickerQuery, Task<TickerResponse>>))
            return Wrap(new GetTickerHandler(market));

        throw new InvalidOperationException($"No handler registered for {handlerType.Name}");
    }

    private ICommandHandler<TRequest, TResponse> Wrap<TRequest, TResponse>(ICommandHandler<TRequest, TResponse> handler)
    {
        // 1. Internal SDK behaviors (if any) could be added here
        
        // 2. Apply user-defined decorator if provided
        if (userDecorator != null)
        {
            var decorated = userDecorator(handler);
            if (decorated is ICommandHandler<TRequest, TResponse> result)
            {
                return result;
            }
            
            // If the user returned something that doesn't implement the interface, 
            // we fall back to the original handler to be safe.
        }

        return handler;
    }
}
