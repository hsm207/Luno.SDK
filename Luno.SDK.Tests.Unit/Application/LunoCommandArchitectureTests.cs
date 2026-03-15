using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Luno.SDK.Account;
using Luno.SDK.Application;
using Luno.SDK.Application.Account;
using Luno.SDK.Application.Market;
using Luno.SDK.Application.Trading;
using Luno.SDK.Market;
using Moq;
using Xunit;

namespace Luno.SDK.Tests.Unit.Application;

/// <summary>
/// Architecture tests for the Command Dispatcher pattern.
/// These tests ensure that the composition root (DefaultCommandHandlerFactory) 
/// stays in sync with our handler implementations.
/// </summary>
public class LunoCommandArchitectureTests
{
    private readonly DefaultCommandHandlerFactory _factory;

    public LunoCommandArchitectureTests()
    {
        // We use mocks for the dependencies of the factory
        _factory = new DefaultCommandHandlerFactory(
            new Mock<ILunoTradingClient>().Object,
            new Mock<ILunoAccountClient>().Object,
            new Mock<ILunoMarketClient>().Object);
    }

    [Fact(DisplayName = "Architecture: The Command Dispatcher must Fail-Fast when a handler is not registered.")]
    public void Factory_UnknownCommand_ThrowsInvalidOperationException()
    {
        // Arrange
        var unknownHandlerType = typeof(ICommandHandler<DummyCommand, Task<string>>);
        
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _factory.CreateHandler(unknownHandlerType, new DummyCommand()));
        Assert.Contains("No handler registered for ICommandHandler`2", ex.Message);
    }

    [Fact(DisplayName = "Architecture: Every ICommandHandler implementation in the assembly must be registered in the Factory.")]
    public void Factory_AllHandlersInAssembly_AreSuccessfullyRegistered()
    {
        // 1. Arrange: Find all concrete handler interfaces in the Application assembly
        var handlerInterfaceDefinition = typeof(ICommandHandler<,>);
        
        // We scan the assembly where the dispatcher lives (Luno.SDK.Application)
        var assembly = typeof(LunoCommandDispatcher).Assembly;

        var handlerInterfacesInAssembly = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract) // Find concrete classes
            .SelectMany(t => t.GetInterfaces())    // Get their interfaces
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceDefinition) // Filter for ICommandHandler<,>
            .Distinct()
            .ToList();

        // 2. Act & Assert: Verification loop
        Assert.NotEmpty(handlerInterfacesInAssembly); // Guard against scan failure

        var failures = new List<string>();

        foreach (var handlerType in handlerInterfacesInAssembly)
        {
            try
            {
                // We don't care about the request object here, just that the factory knows how to create the handler type
                var handler = _factory.CreateHandler(handlerType, null!);
                Assert.NotNull(handler);
            }
            catch (Exception ex)
            {
                failures.Add($"{handlerType.Name}: {ex.Message}");
            }
        }

        // 3. Final Assertion
        if (failures.Count > 0)
        {
            var summary = string.Join(Environment.NewLine, failures);
            Assert.Fail($"Architecture Violation! The following handlers are missing from DefaultCommandHandlerFactory:{Environment.NewLine}{summary}");
        }
    }

    [Fact(DisplayName = "Architecture: The Factory must apply the user-provided decorator to handlers.")]
    public void Factory_WithUserDecorator_AppliesDecoration()
    {
        // Arrange
        var decoratorCalled = false;
        var factoryWithDecorator = new DefaultCommandHandlerFactory(
            new Mock<ILunoTradingClient>().Object,
            new Mock<ILunoAccountClient>().Object,
            new Mock<ILunoMarketClient>().Object,
            handler =>
            {
                decoratorCalled = true;
                return handler; // Passthrough
            });

        var handlerType = typeof(ICommandHandler<GetTickerQuery, Task<TickerResponse>>);

        // Act
        var handler = factoryWithDecorator.CreateHandler(handlerType, null!);

        // Assert
        Assert.NotNull(handler);
        Assert.True(decoratorCalled, "The user decorator was not called!");
    }

    // A dummy command for testing the fail-fast guard
    private class DummyCommand { }
}
