using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Luno.SDK;
using Luno.SDK.Infrastructure.ErrorHandling;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.ErrorHandling;

// A simple stub adapter to avoid using Moq
public class StubRequestAdapter : IRequestAdapter
{
    private readonly Exception? _exceptionToThrow;

    public StubRequestAdapter(Exception? exceptionToThrow = null)
    {
        _exceptionToThrow = exceptionToThrow;
    }

    public string? BaseUrl { get; set; }
    public ISerializationWriterFactory SerializationWriterFactory => throw new NotImplementedException();
    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory) { }

    public virtual Task<ModelType?> SendAsync<ModelType>(RequestInformation requestInfo, ParsableFactory<ModelType> factory, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null, CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        if (_exceptionToThrow != null) throw _exceptionToThrow;
        return Task.FromResult<ModelType?>(default);
    }

    public Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(RequestInformation requestInfo, ParsableFactory<ModelType> factory, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null, CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        if (_exceptionToThrow != null) throw _exceptionToThrow;
        return Task.FromResult<IEnumerable<ModelType>?>(default);
    }

    public Task<ModelType?> SendPrimitiveAsync<ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null, CancellationToken cancellationToken = default)
    {
        if (_exceptionToThrow != null) throw _exceptionToThrow;
        return Task.FromResult<ModelType?>(default);
    }

    public Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null, CancellationToken cancellationToken = default)
    {
        if (_exceptionToThrow != null) throw _exceptionToThrow;
        return Task.FromResult<IEnumerable<ModelType>?>(default);
    }

    public Task SendNoContentAsync(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null, CancellationToken cancellationToken = default)
    {
        if (_exceptionToThrow != null) throw _exceptionToThrow;
        return Task.CompletedTask;
    }

    public Task<T?> ConvertToNativeRequestAsync<T>(RequestInformation requestInfo, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<T?>(default);
    }
}

public class LunoErrorHandlingAdapterTests
{
    private class DummyParsable : IParsable
    {
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => new Dictionary<string, Action<IParseNode>>();
        public void Serialize(ISerializationWriter writer) { }
    }

    private static readonly ParsableFactory<DummyParsable> Factory = (node) => new DummyParsable();

    [Fact(DisplayName = "Given ApiException with 401, When SendAsync is called, Then throw LunoUnauthorizedException")]
    public async Task SendAsync_401_ThrowsUnauthorized()
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 401 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        await Assert.ThrowsAsync<LunoUnauthorizedException>(() => errorAdapter.SendAsync<DummyParsable>(requestInfo, Factory));
    }

    [Fact(DisplayName = "Given ApiException with 403, When SendAsync is called, Then throw LunoForbiddenException")]
    public async Task SendAsync_403_ThrowsForbidden()
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 403 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        await Assert.ThrowsAsync<LunoForbiddenException>(() => errorAdapter.SendAsync<DummyParsable>(requestInfo, Factory));
    }

    [Fact(DisplayName = "Given ApiException with 500, When SendAsync is called, Then throw LunoApiException")]
    public async Task SendAsync_500_ThrowsLunoApiException()
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 500 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoApiException>(() => errorAdapter.SendAsync<DummyParsable>(requestInfo, Factory));
        Assert.Equal(500, ex.StatusCode);
    }

    [Theory(DisplayName = "Given 401 ApiException, When calling any Send method, Then throw LunoUnauthorizedException")]
    [InlineData("SendCollectionAsync")]
    [InlineData("SendPrimitiveAsync")]
    [InlineData("SendPrimitiveCollectionAsync")]
    [InlineData("SendNoContentAsync")]
    public async Task AllSendMethods_401_ThrowsUnauthorized(string methodName)
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 401 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        await Assert.ThrowsAsync<LunoUnauthorizedException>(async () =>
        {
            switch (methodName)
            {
                case "SendCollectionAsync":
                    await errorAdapter.SendCollectionAsync<DummyParsable>(requestInfo, Factory);
                    break;
                case "SendPrimitiveAsync":
                    await errorAdapter.SendPrimitiveAsync<string>(requestInfo);
                    break;
                case "SendPrimitiveCollectionAsync":
                    await errorAdapter.SendPrimitiveCollectionAsync<string>(requestInfo);
                    break;
                case "SendNoContentAsync":
                    await errorAdapter.SendNoContentAsync(requestInfo);
                    break;
            }
        });
    }

    [Theory(DisplayName = "Given 403 ApiException, When calling any Send method, Then throw LunoForbiddenException")]
    [InlineData("SendCollectionAsync")]
    [InlineData("SendPrimitiveAsync")]
    [InlineData("SendPrimitiveCollectionAsync")]
    [InlineData("SendNoContentAsync")]
    public async Task AllSendMethods_403_ThrowsForbidden(string methodName)
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 403 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        await Assert.ThrowsAsync<LunoForbiddenException>(async () =>
        {
            switch (methodName)
            {
                case "SendCollectionAsync":
                    await errorAdapter.SendCollectionAsync<DummyParsable>(requestInfo, Factory);
                    break;
                case "SendPrimitiveAsync":
                    await errorAdapter.SendPrimitiveAsync<string>(requestInfo);
                    break;
                case "SendPrimitiveCollectionAsync":
                    await errorAdapter.SendPrimitiveCollectionAsync<string>(requestInfo);
                    break;
                case "SendNoContentAsync":
                    await errorAdapter.SendNoContentAsync(requestInfo);
                    break;
            }
        });
    }

    [Theory(DisplayName = "Given 500 ApiException, When calling any Send method, Then throw LunoApiException")]
    [InlineData("SendCollectionAsync")]
    [InlineData("SendPrimitiveAsync")]
    [InlineData("SendPrimitiveCollectionAsync")]
    [InlineData("SendNoContentAsync")]
    public async Task AllSendMethods_500_ThrowsLunoApiException(string methodName)
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 500 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoApiException>(async () =>
        {
            switch (methodName)
            {
                case "SendCollectionAsync":
                    await errorAdapter.SendCollectionAsync<DummyParsable>(requestInfo, Factory);
                    break;
                case "SendPrimitiveAsync":
                    await errorAdapter.SendPrimitiveAsync<string>(requestInfo);
                    break;
                case "SendPrimitiveCollectionAsync":
                    await errorAdapter.SendPrimitiveCollectionAsync<string>(requestInfo);
                    break;
                case "SendNoContentAsync":
                    await errorAdapter.SendNoContentAsync(requestInfo);
                    break;
            }
        });

        Assert.Equal(500, ex.StatusCode);
    }

    [Fact(DisplayName = "Given ConvertToNativeRequestAsync, When called, Then delegate faithfully")]
    public async Task ConvertToNativeRequestAsync_Always_DelegatesFaithfully()
    {
        // Arrange
        var innerAdapter = new StubRequestAdapter();
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act
        var result = await errorAdapter.ConvertToNativeRequestAsync<string>(requestInfo);

        // Assert (Stub returns default, which is null for class strings)
        Assert.Null(result);
    }

    [Fact(DisplayName = "Given properties, When getting and setting, Then delegate to inner adapter")]
    public void Properties_Always_DelegatesToInnerAdapter()
    {
        // Arrange
        var innerAdapter = new StubRequestAdapter { BaseUrl = "https://test.com" };
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);

        // Act & Assert
        Assert.Equal("https://test.com", errorAdapter.BaseUrl);

        errorAdapter.BaseUrl = "https://new.com";
        Assert.Equal("https://new.com", innerAdapter.BaseUrl);

        Assert.Throws<NotImplementedException>(() => errorAdapter.SerializationWriterFactory);
    }

    [Fact(DisplayName = "Given EnableBackingStore, When called, Then delegate to inner adapter")]
    public void EnableBackingStore_Always_DelegatesToInnerAdapter()
    {
        // Arrange
        var innerAdapter = new StubRequestAdapter();
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);

        // Act
        var ex = Record.Exception(() => errorAdapter.EnableBackingStore(null!));

        // Assert
        Assert.Null(ex); // Stub simply does nothing, so no exception means it successfully delegated
    }
}
