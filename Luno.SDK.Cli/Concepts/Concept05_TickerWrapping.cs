using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Luno.SDK;
using Luno.SDK.Application;
using Luno.SDK.Application.Market;
using Luno.SDK.Market;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a demonstration of wrapping SDK functionality with custom pipeline behaviors.
/// </summary>
public static class Concept05_TickerWrapping
{
    public static async Task RunAsync()
    {
        Console.WriteLine("--- Demonstration: Pipeline Behaviors (Middleware) ---");

        // 1. Setup DI with a custom behavior
        var services = new ServiceCollection();
        services.AddLunoClient();

        // Register our logging behavior for the ticker query
        services.AddTransient<IPipelineBehavior<GetTickerQuery, TickerResponse>, TickerLoggingBehavior>();

        var sp = services.BuildServiceProvider();
        var luno = sp.GetRequiredService<ILunoClient>();

        Console.WriteLine("📡 Fetching ticker price with 'TickerLoggingBehavior' active...");

        try
        {
            var ticker = await luno.Market.GetTickerAsync(new GetTickerQuery("XBTZAR"));
            Console.WriteLine($"[RESULT] {ticker.Pair}: {ticker.Price:N2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }

        Console.WriteLine("--- Demonstration Complete ---");
    }

    /// <summary>
    /// A custom pipeline behavior that logs the execution of a ticker request.
    /// This follows the modern 'Middleware' pattern.
    /// </summary>
    private class TickerLoggingBehavior : IPipelineBehavior<GetTickerQuery, TickerResponse>
    {
        public async Task<TickerResponse> Handle(
            GetTickerQuery request,
            RequestHandlerDelegate<TickerResponse> next,
            CancellationToken ct)
        {
            var startTime = DateTime.UtcNow;
            Console.WriteLine($"[PIPELINE] >>> Starting request for {request.Pair} at {startTime:HH:mm:ss.fff}");

            try
            {
                // Call the next step in the pipeline (could be another behavior or the actual handler)
                var response = await next();

                var duration = DateTime.UtcNow - startTime;
                Console.WriteLine($"[PIPELINE] <<< Finished request in {duration.TotalMilliseconds}ms");

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PIPELINE] !!! Request failed: {ex.Message}");
                throw;
            }
        }
    }
}
