using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Moq;
using Luno.SDK.Infrastructure.Telemetry;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class TelemetryAdapterTests
{
    private readonly Mock<IRequestAdapter> _innerMock;
    private readonly LunoTelemetry _telemetry;
    private readonly LunoTelemetryAdapter _adapter;

    public TelemetryAdapterTests()
    {
        _innerMock = new Mock<IRequestAdapter>();
        _telemetry = new LunoTelemetry();
        _adapter = new LunoTelemetryAdapter(_innerMock.Object, _telemetry, NullLogger.Instance);
    }

    [Fact(DisplayName = "Given telemetry adapter, When delegating properties, Then call inner adapter faithfully.")]
    public void PropertiesWhenDelegatingShouldCallInner()
    {
        var factoryMock = new Mock<ISerializationWriterFactory>();
        _innerMock.Setup(x => x.SerializationWriterFactory).Returns(factoryMock.Object);
        Assert.Equal(factoryMock.Object, _adapter.SerializationWriterFactory);

        _innerMock.SetupProperty(x => x.BaseUrl, "old");
        Assert.Equal("old", _adapter.BaseUrl);
        _adapter.BaseUrl = "new";
        Assert.Equal("new", _innerMock.Object.BaseUrl);
    }

    [Fact(DisplayName = "Given telemetry adapter, When calling SendAsync, Then record telemetry and delegate.")]
    public async Task SendAsyncWhenCalledShouldRecordAndDelegate()
    {
        var requestInfo = new RequestInformation();
        var factoryMock = new Mock<ParsableFactory<IParsable>>();
        _innerMock.Setup(x => x.SendAsync(It.IsAny<RequestInformation>(), It.IsAny<ParsableFactory<IParsable>>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IParsable>().Object);

        await _adapter.SendAsync(requestInfo, factoryMock.Object);

        _innerMock.Verify(x => x.SendAsync(requestInfo, factoryMock.Object, null, default), Times.Once);
    }

    [Fact(DisplayName = "Given telemetry adapter, When calling SendPrimitiveAsync, Then record telemetry and delegate.")]
    public async Task SendPrimitiveAsyncWhenCalledShouldRecordAndDelegate()
    {
        var requestInfo = new RequestInformation();
        _innerMock.Setup(x => x.SendPrimitiveAsync<string>(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Success");

        var result = await _adapter.SendPrimitiveAsync<string>(requestInfo);

        Assert.Equal("Success", result);
        _innerMock.Verify(x => x.SendPrimitiveAsync<string>(requestInfo, null, default), Times.Once);
    }

    [Fact(DisplayName = "Given telemetry adapter, When calling SendPrimitiveCollectionAsync, Then record telemetry and delegate.")]
    public async Task SendPrimitiveCollectionAsyncWhenCalledShouldRecordAndDelegate()
    {
        var requestInfo = new RequestInformation();
        _innerMock.Setup(x => x.SendPrimitiveCollectionAsync<string>(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        await _adapter.SendPrimitiveCollectionAsync<string>(requestInfo);

        _innerMock.Verify(x => x.SendPrimitiveCollectionAsync<string>(requestInfo, null, default), Times.Once);
    }

    [Fact(DisplayName = "Given telemetry adapter, When calling SendNoContentAsync, Then record telemetry and delegate.")]
    public async Task SendNoContentAsyncWhenCalledShouldRecordAndDelegate()
    {
        var requestInfo = new RequestInformation();
        _innerMock.Setup(x => x.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _adapter.SendNoContentAsync(requestInfo);

        _innerMock.Verify(x => x.SendNoContentAsync(requestInfo, null, default), Times.Once);
    }

    [Fact(DisplayName = "Given telemetry adapter, When calling ConvertToNativeRequestAsync, Then delegate faithfully.")]
    public async Task ConvertToNativeWhenCalledShouldDelegate()
    {
        var requestInfo = new RequestInformation();
        _innerMock.Setup(x => x.ConvertToNativeRequestAsync<string>(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Native");

        var result = await _adapter.ConvertToNativeRequestAsync<string>(requestInfo);

        Assert.Equal("Native", result);
    }

    [Fact(DisplayName = "Given operation fails, When calling SendAsync, Then record error and throw.")]
    public async Task SendAsyncWhenFailsShouldRecordErrorAndThrow()
    {
        var requestInfo = new RequestInformation();
        _innerMock.Setup(x => x.SendAsync(It.IsAny<RequestInformation>(), It.IsAny<ParsableFactory<IParsable>>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _adapter.SendAsync(requestInfo, new Mock<ParsableFactory<IParsable>>().Object));
    }
}
