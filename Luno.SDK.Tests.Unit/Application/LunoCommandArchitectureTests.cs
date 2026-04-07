using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK;
using Luno.SDK.Account;
using Luno.SDK.Application;
using Luno.SDK.Application.Account;
using Luno.SDK.Application.Market;
using Luno.SDK.Application.Trading;
using Luno.SDK.Market;
using Luno.SDK.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Luno.SDK.Tests.Unit.Application;

/// <summary>
/// Architecture tests for the Command Dispatcher pattern.
/// These tests ensure that the composition root (LunoServiceExtensions) 
/// stays in sync with our handler implementations and enforces the pipeline pattern.
/// </summary>
public class LunoCommandArchitectureTests
{
    private readonly IServiceCollection _services;

    public LunoCommandArchitectureTests()
    {
        _services = new ServiceCollection();
        _services.AddLunoClient(opt => {
            opt.ApiKeyId = "key";
            opt.ApiKeySecret = "secret";
        });
        
        // Setup mocks for required infrastructure dependencies
        // (These are normally registered by AddLunoClient but we might need to override for pure architecture tests)
    }

    [Fact(DisplayName = "Architecture: The Command Dispatcher must Fail-Fast when a handler is not registered.")]
    public async Task Dispatcher_UnknownCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        var dispatcher = new LunoCommandDispatcher(type => null); // Resolver returns nothing
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.DispatchAsync<DummyCommand, string>(new DummyCommand()));
        Assert.Contains("No handler registered for ICommandHandler`2", ex.Message);
    }

    [Fact(DisplayName = "Architecture: Every ICommandHandler implementation in the assembly must be registered via AddLunoClient.")]
    public void DI_AllHandlersInAssembly_AreSuccessfullyRegistered()
    {
        // 1. Arrange: Find all concrete handler interfaces in the Application assembly
        var handlerInterfaceDefinition = typeof(ICommandHandler<,>);
        var streamHandlerInterfaceDefinition = typeof(IStreamCommandHandler<,>);
        var assembly = typeof(LunoCommandDispatcher).Assembly;

        var handlerInterfacesInAssembly = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == handlerInterfaceDefinition || i.GetGenericTypeDefinition() == streamHandlerInterfaceDefinition))
            .Distinct()
            .ToList();

        var sp = _services.BuildServiceProvider();

        // 2. Act & Assert: Verification loop
        Assert.NotEmpty(handlerInterfacesInAssembly);

        var failures = new List<string>();
        foreach (var handlerType in handlerInterfacesInAssembly)
        {
            var handler = sp.GetService(handlerType);
            if (handler == null)
            {
                failures.Add(handlerType.Name);
            }
        }

        // 3. Final Assertion
        if (failures.Count > 0)
        {
            var summary = string.Join(", ", failures);
            Assert.Fail($"Architecture Violation! The following handlers are missing from DI registration: {summary}");
        }
    }

    [Fact(DisplayName = "Architecture: The Dispatcher must execute registered pipeline behaviors in order.")]
    public async Task Dispatcher_WithBehaviors_ExecutesPipeline()
    {
        // Arrange
        var executionFullTrace = new List<string>();
        
        var services = new ServiceCollection();
        // Register a mock handler
        var handlerMock = new Mock<ICommandHandler<GetTickerQuery, TickerResponse>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<GetTickerQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() => {
                       executionFullTrace.Add("Handler");
                       return new TickerResponse("XBTZAR", 1000m, 1005m, 995m, 10m, true, DateTimeOffset.UtcNow);
                   });
        
        services.AddTransient(_ => handlerMock.Object);
        
        // Register behaviors
        services.AddLunoCommandBehavior(typeof(TraceBehavior<,>));
        
        var sp = services.BuildServiceProvider();
        var dispatcher = new LunoCommandDispatcher(type => sp.GetService(type));
        TraceBehavior<GetTickerQuery, TickerResponse>.Trace = executionFullTrace;

        // Act
        await dispatcher.DispatchAsync<GetTickerQuery, TickerResponse>(new GetTickerQuery("XBTZAR"));

        // Assert
        Assert.Equal(3, executionFullTrace.Count);
        Assert.Equal("Start", executionFullTrace[0]);
        Assert.Equal("Handler", executionFullTrace[1]);
        Assert.Equal("End", executionFullTrace[2]);
    }

    [Fact(DisplayName = "Architecture: The Dispatcher must execute stream pipeline behaviors.")]
    public async Task Dispatcher_WithStreamBehaviors_ExecutesPipeline()
    {
        // Arrange
        var executionFullTrace = new List<string>();
        var services = new ServiceCollection();
        
        var handlerMock = new Mock<IStreamCommandHandler<GetTickersQuery, TickerResponse>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<GetTickersQuery>(), It.IsAny<CancellationToken>()))
                   .Returns(new[] { new TickerResponse("XBTZAR", 1000m, 1005m, 995m, 10m, true, DateTimeOffset.UtcNow) }.ToAsyncEnumerable());
        
        services.AddTransient(_ => handlerMock.Object);
        services.AddLunoStreamBehavior(typeof(StreamTraceBehavior<,>));
        
        var sp = services.BuildServiceProvider();
        var dispatcher = new LunoCommandDispatcher(type => sp.GetService(type));
        StreamTraceBehavior<GetTickersQuery, TickerResponse>.Trace = executionFullTrace;

        // Act
        await foreach (var item in dispatcher.CreateStreamAsync<GetTickersQuery, TickerResponse>(new GetTickersQuery()))
        {
            executionFullTrace.Add("Item");
        }

        // Assert
        Assert.Equal(4, executionFullTrace.Count);
        Assert.Equal("Stream Start", executionFullTrace[0]);
        Assert.Equal("Item", executionFullTrace[1]);
        Assert.Equal("Stream End", executionFullTrace[2]);
        Assert.Equal("Stream Finally", executionFullTrace[3]);
    }

    private class DummyCommand { }

    private class TraceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public static List<string> Trace { get; set; } = new();

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            Trace.Add("Start");
            var result = await next();
            Trace.Add("End");
            return result;
        }
    }

    private class StreamTraceBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    {
        public static List<string> Trace { get; set; } = new();

        public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken ct)
        {
            Trace.Add("Stream Start");
            try
            {
                await foreach (var item in next())
                {
                    yield return item;
                }
                Trace.Add("Stream End");
            }
            finally
            {
                Trace.Add("Stream Finally");
            }
        }
    }
}
