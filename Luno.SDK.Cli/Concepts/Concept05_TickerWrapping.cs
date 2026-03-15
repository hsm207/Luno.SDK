using System;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK;
using Luno.SDK.Application;
using Luno.SDK.Application.Market;
using Luno.SDK.Market;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a demonstration of how end-users can wrap SDK command handlers with custom decorators.
/// This illustrates the power of the Command Dispatcher pattern for cross-cutting concerns.
/// </summary>
public static class Concept05_TickerWrapping
{
    public static async Task RunAsync()
    {
        Console.WriteLine("--- Demonstration: User-Defined Handler Decorators ---");

        // 1. Initialize the client with a custom Command Handler Decorator
        var options = new LunoClientOptions
        {
            // The decorator is a function that receives a handler and can return a wrapped version
            CommandHandlerDecorator = handler =>
            {
                // We only want to wrap the GetTickerQuery handler in this example
                if (handler is ICommandHandler<GetTickerQuery, Task<TickerResponse>> tickerHandler)
                {
                    return new TickerLoggingDecorator(tickerHandler);
                }

                return handler; // Return as-is for everything else
            }
        };

        var luno = new LunoClient(options);

        // 2. Call the endpoint as usual
        // The SDK will automatically apply our decorator inside the dispatcher!
        Console.WriteLine("📡 Fetching ticker for XBTZAR...");
        var ticker = await luno.Market.GetTickerAsync("XBTZAR");

        Console.WriteLine($"✅ Result: {ticker.Pair} Price: {ticker.Price}");
        Console.WriteLine("--- Demonstration Complete ---");
    }

    /// <summary>
    /// A simple decorator that logs the start and end of a GetTicker operation.
    /// In a real app, this could be a Polly retry policy or a custom telemetry sink!
    /// </summary>
    private class TickerLoggingDecorator(ICommandHandler<GetTickerQuery, Task<TickerResponse>> inner) 
        : ICommandHandler<GetTickerQuery, Task<TickerResponse>>
    {
        public async Task<TickerResponse> HandleAsync(GetTickerQuery request, CancellationToken ct = default)
        {
            var start = DateTime.UtcNow;
            Console.WriteLine($"[LOG] Starting request for {request.Pair} at {start:HH:mm:ss.fff}");
            
            try 
            {
                var result = await inner.HandleAsync(request, ct);
                
                var duration = DateTime.UtcNow - start;
                Console.WriteLine($"[LOG] Request for {request.Pair} completed in {duration.TotalMilliseconds:N0}ms");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG] Request for {request.Pair} FAILED: {ex.Message}");
                throw;
            }
        }
    }
}
