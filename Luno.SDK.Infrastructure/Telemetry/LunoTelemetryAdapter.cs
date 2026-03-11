using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;

namespace Luno.SDK.Infrastructure.Telemetry;

/// <summary>
/// A decorator for <see cref="IRequestAdapter"/> that handles telemetry recording and centralized error mapping.
/// </summary>
internal class LunoTelemetryAdapter(IRequestAdapter inner, LunoTelemetry telemetry, ILogger logger) : IRequestAdapter
{
    public ISerializationWriterFactory SerializationWriterFactory => inner.SerializationWriterFactory;

    public string? BaseUrl { get => inner.BaseUrl; set => inner.BaseUrl = value; }

    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory) => inner.EnableBackingStore(backingStoreFactory);

    public Task<ModelType?> SendAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        return WrapAsync(() => inner.SendAsync(requestInfo, factory, errorMapping, cancellationToken), requestInfo);
    }

    public Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        return WrapAsync(() => inner.SendCollectionAsync(requestInfo, factory, errorMapping, cancellationToken), requestInfo);
    }

    public Task<ModelType?> SendPrimitiveAsync<ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default)
    {
        return WrapAsync(() => inner.SendPrimitiveAsync<ModelType>(requestInfo, errorMapping, cancellationToken), requestInfo);
    }

    public Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default)
    {
        return WrapAsync(() => inner.SendPrimitiveCollectionAsync<ModelType>(requestInfo, errorMapping, cancellationToken), requestInfo);
    }

    public Task SendNoContentAsync(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
        CancellationToken cancellationToken = default)
    {
        return WrapAsync(async () =>
        {
            await inner.SendNoContentAsync(requestInfo, errorMapping, cancellationToken);
            return true; // Dummy value for the wrapper
        }, requestInfo);
    }

    public Task<T?> ConvertToNativeRequestAsync<T>(
        RequestInformation requestInfo,
        CancellationToken cancellationToken = default)
    {
        // Conversion methods typically don't record telemetry as they don't hit the wire,
        // but we'll delegate it faithfully.
        return inner.ConvertToNativeRequestAsync<T>(requestInfo, cancellationToken);
    }

    private async Task<T?> WrapAsync<T>(Func<Task<T?>> action, RequestInformation requestInfo)
    {
        var options = requestInfo.RequestOptions.OfType<LunoTelemetryOptions>().FirstOrDefault();
        var operationName = options?.OperationName ?? "UnknownOperation";

        using var activity = telemetry.ActivitySource.StartActivity(operationName);
        activity?.SetTag("luno.operation", operationName);

        var stopwatch = Stopwatch.StartNew();

        logger.LogDebug("Executing decorated operation '{OperationName}'", operationName);

        try
        {
            var result = await action();
            activity?.SetTag("luno.status", "Success");
            telemetry.RecordRequest(operationName, "Success");
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("luno.status", "Error");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            telemetry.RecordRequest(operationName, "Error");
            logger.LogError(ex, "Operation '{OperationName}' failed.", operationName);
            throw;
        }
        finally
        {
            telemetry.RecordDuration(stopwatch.Elapsed.TotalMilliseconds, operationName);
        }
    }
}
