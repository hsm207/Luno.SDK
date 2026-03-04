using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Luno.SDK.Core.Account;
using Luno.SDK.Infrastructure.Account;
using Luno.SDK.Infrastructure.Authentication;
using Luno.SDK.Tests.Unit.Infrastructure.ErrorHandling; // Assuming StubRequestAdapter is public and reusable
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Account;

public class LunoAccountClientArchitectureTests
{
    // A mock-free request adapter to intercept requests and capture options
    private class InspectingRequestAdapter : StubRequestAdapter
    {
        public RequestInformation? LastRequest { get; private set; }

#pragma warning disable CS8613
#pragma warning disable CS8714
#pragma warning disable CS8609
#pragma warning disable CS8619
        public override Task<ModelType> SendAsync<ModelType>(
            RequestInformation requestInfo,
            Microsoft.Kiota.Abstractions.Serialization.ParsableFactory<ModelType> factory,
            System.Collections.Generic.Dictionary<string, Microsoft.Kiota.Abstractions.Serialization.ParsableFactory<Microsoft.Kiota.Abstractions.Serialization.IParsable>>? errorMapping = null,
            CancellationToken cancellationToken = default)
        {
            LastRequest = requestInfo;
            // Return an empty model that passes null checks if needed, or null if expected.
            // Throwing or returning null is fine as long as the request is intercepted.
            // Returning null causes standard response parsing to fail later, but the request is already captured.
            return Task.FromResult<ModelType>(default!);
        }
    }

    [Fact(DisplayName = "Given LunoAccountClient, When calling any method, Then ensure LunoAuthenticationOption is applied")]
    public async Task AllMethods_GivenLunoAccountClient_WhenCalled_ThenApplyAuthenticationOption()
    {
        // Arrange
        var adapter = new InspectingRequestAdapter();
        // Since LunoAccountClient is internal, we use reflection to instantiate it.
        var clientType = typeof(LunoAccountClient);
        var client = (ILunoAccountClient)Activator.CreateInstance(clientType, adapter)!;

        // Get all public methods declared on ILunoAccountClient that return a Task
        var methods = typeof(ILunoAccountClient).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => typeof(Task).IsAssignableFrom(m.ReturnType));

        Assert.NotEmpty(methods);

        foreach (var method in methods)
        {
            // Reset LastRequest for each method
            var propertyInfo = adapter.GetType().GetProperty(nameof(InspectingRequestAdapter.LastRequest));
            propertyInfo?.SetValue(adapter, null);

            // Construct default arguments for the method
            var parameters = method.GetParameters();
            var args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = parameters[i].ParameterType.IsValueType
                    ? Activator.CreateInstance(parameters[i].ParameterType)
                    : null;
            }

            // Act
            try
            {
                var task = (Task)method.Invoke(client, args)!;
                await task;
            }
            catch (Exception)
            {
                // We expect exceptions (e.g., InvalidOperationException from returning null response)
                // We just care that the request reached the adapter before failing.
            }

            // Assert
            Assert.NotNull(adapter.LastRequest);
            var authOption = adapter.LastRequest.RequestOptions.OfType<LunoAuthenticationOption>().FirstOrDefault();

            Assert.NotNull(authOption);
            Assert.True(authOption.RequiresAuthentication, $"Method {method.Name} failed to set RequiresAuthentication = true.");
        }
    }
}
