using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK;
using Luno.SDK.Account;
using Luno.SDK.Application;
using Luno.SDK.Application.Account;
using Luno.SDK.Application.Market;
using Luno.SDK.Application.Trading;
using Luno.SDK.Market;
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
    public void Dispatcher_UnknownCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        var dispatcher = new LunoCommandDispatcher(type => null); // Resolver returns nothing
        
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => dispatcher.DispatchAsync<DummyCommand, string>(new DummyCommand()));
        Assert.Contains("No handler registered for ICommandHandler`2", ex.Message);
    }

    [Fact(DisplayName = "Architecture: Every ICommandHandler implementation in the assembly must be registered via AddLunoClient.")]
    public void DI_AllHandlersInAssembly_AreSuccessfullyRegistered()
    {
        // 1. Arrange: Find all concrete handler interfaces in the Application assembly
        var handlerInterfaceDefinition = typeof(ICommandHandler<,>);
        var assembly = typeof(LunoCommandDispatcher).Assembly;

        var handlerInterfacesInAssembly = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceDefinition)
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

    [Fact(DisplayName = "Architecture: The Dispatcher must execute registered pipeline behaviors (including open generics) in order.")]
    public async Task Dispatcher_WithBehaviors_ExecutesPipeline()
    {
        // Arrange
        var executionFullTrace = new List<string>();
        GlobalTraceBehavior.Trace = executionFullTrace; // Reset static trace for global behavior
        
        var services = new ServiceCollection();
        // Register a mock handler
        var handlerMock = new Mock<ICommandHandler<GetTickerQuery, Task<TickerResponse>>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<GetTickerQuery>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() => {
                       executionFullTrace.Add("Handler");
                       return new TickerResponse("XBTZAR", 1000m, 1005m, 995m, 10m, true, DateTimeOffset.UtcNow);
                   });
        
        services.AddTransient(_ => handlerMock.Object);
        
        // Register behaviors using the public extension methods
        services.AddLunoCommandBehavior(typeof(GlobalTraceBehavior<,>)); // Open generic 1
        services.AddLunoCommandBehavior(typeof(LocalTraceBehavior<,>));  // Open generic 2
        
        var sp = services.BuildServiceProvider();
        var dispatcher = new LunoCommandDispatcher(type => sp.GetService(type));

        // Act
        await dispatcher.DispatchAsync<GetTickerQuery, Task<TickerResponse>>(new GetTickerQuery("XBTZAR"));

        // Assert
        Assert.Equal(5, executionFullTrace.Count); // Global start, Local start, Handler, Local end, Global end
        Assert.Equal("Global Start", executionFullTrace[0]);
        Assert.Equal("Local Start", executionFullTrace[1]);
        Assert.Equal("Handler", executionFullTrace[2]);
        Assert.Equal("Local End", executionFullTrace[3]);
        Assert.Equal("Global End", executionFullTrace[4]);
    }

    private class DummyCommand { }

    private class GlobalTraceBehavior
    {
        public static List<string> Trace = new();
    }

    private class GlobalTraceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public TResponse Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            GlobalTraceBehavior.Trace.Add("Global Start");
            var result = next();
            GlobalTraceBehavior.Trace.Add("Global End");
            return result;
        }
    }

    private class LocalTraceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public TResponse Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            GlobalTraceBehavior.Trace.Add("Local Start");
            var result = next();
            GlobalTraceBehavior.Trace.Add("Local End");
            return result;
        }
    }
}
