using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;

namespace Luno.SDK.Infrastructure.ErrorHandling;

/// <summary>
/// A decorator for <see cref="IRequestAdapter"/> that intercepts <see cref="ApiException"/>
/// and translates specific HTTP status codes (e.g., 401, 403) into domain-specific exceptions.
/// </summary>
public class LunoErrorHandlingAdapter : IRequestAdapter
{
    private readonly IRequestAdapter _innerAdapter;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoErrorHandlingAdapter"/> class.
    /// </summary>
    /// <param name="innerAdapter">The inner request adapter to delegate calls to.</param>
    public LunoErrorHandlingAdapter(IRequestAdapter innerAdapter)
    {
        _innerAdapter = innerAdapter ?? throw new ArgumentNullException(nameof(innerAdapter));
    }

    /// <inheritdoc />
    public string? BaseUrl
    {
        get => _innerAdapter.BaseUrl;
        set => _innerAdapter.BaseUrl = value;
    }

    /// <inheritdoc />
    public ISerializationWriterFactory SerializationWriterFactory => _innerAdapter.SerializationWriterFactory;

    /// <inheritdoc />
    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory) =>
        _innerAdapter.EnableBackingStore(backingStoreFactory);

    /// <inheritdoc />
    public async Task<ModelType?> SendAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        try
        {
            return await _innerAdapter.SendAsync(requestInfo, factory, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        try
        {
            return await _innerAdapter.SendCollectionAsync(requestInfo, factory, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T?> SendPrimitiveAsync<T>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _innerAdapter.SendPrimitiveAsync<T>(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>?> SendPrimitiveCollectionAsync<T>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _innerAdapter.SendPrimitiveCollectionAsync<T>(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendNoContentAsync(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _innerAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<T?> ConvertToNativeRequestAsync<T>(
        RequestInformation requestInfo,
        CancellationToken cancellationToken = default)
    {
        return _innerAdapter.ConvertToNativeRequestAsync<T>(requestInfo, cancellationToken);
    }

    private void HandleException(ApiException ex)
    {
        if (ex.ResponseStatusCode == 401)
        {
            throw new LunoUnauthorizedException("Invalid API credentials. Verify your API Key ID and Secret.", ex);
        }
        if (ex.ResponseStatusCode == 403)
        {
            throw new LunoForbiddenException("The provided API credentials do not have permission to access this resource.", ex);
        }
    }
}
