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
    public async Task SendAsync_Given401_WhenCalled_ThenThrowUnauthorized()
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
    public async Task SendAsync_Given403_WhenCalled_ThenThrowForbidden()
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 403 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        await Assert.ThrowsAsync<LunoForbiddenException>(() => errorAdapter.SendAsync<DummyParsable>(requestInfo, Factory));
    }

    [Fact(DisplayName = "Given ApiException with 500, When SendAsync is called, Then re-throw ApiException")]
    public async Task SendAsync_Given500_WhenCalled_ThenRethrowApiEx()
    {
        // Arrange
        var apiEx = new ApiException { ResponseStatusCode = 500 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => errorAdapter.SendAsync<DummyParsable>(requestInfo, Factory));
        Assert.Equal(500, ex.ResponseStatusCode);
    }
}
