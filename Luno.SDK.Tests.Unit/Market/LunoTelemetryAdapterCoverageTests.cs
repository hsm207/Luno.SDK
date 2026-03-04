using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Moq;
using Xunit;
using Luno.SDK.Infrastructure.Telemetry;

namespace Luno.SDK.Tests.Unit.Market;

public class LunoTelemetryAdapterCoverageTests
{
    private class DummyModel : IParsable
    {
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => new Dictionary<string, Action<IParseNode>>();
        public void Serialize(ISerializationWriter writer) { }
    }

    private class DummyParsableFactory
    {
        public static DummyModel CreateFromDiscriminatorValue(IParseNode parseNode) => new();
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When calling SendAsync, Then call inner adapter and return result")]
    public async Task SendAsync_GivenLunoTelemetryAdapter_WhenCallingSendAsync_ThenCallInnerAdapterAndReturnResult()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);
        var requestInfo = new RequestInformation();
        requestInfo.AddRequestOptions(new IRequestOption[] { new LunoTelemetryOptions("TestOperation") });
        var expectedResult = new DummyModel();

        innerMock.Setup(m => m.SendAsync(requestInfo, It.IsAny<ParsableFactory<DummyModel>>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await adapter.SendAsync(requestInfo, DummyParsableFactory.CreateFromDiscriminatorValue);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When calling SendCollectionAsync, Then call inner adapter and return result")]
    public async Task SendCollectionAsync_GivenLunoTelemetryAdapter_WhenCallingSendCollectionAsync_ThenCallInnerAdapterAndReturnResult()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);
        var requestInfo = new RequestInformation();
        requestInfo.AddRequestOptions(new IRequestOption[] { new LunoTelemetryOptions("TestOperation") });
        var expectedResult = new List<DummyModel> { new() };

        innerMock.Setup(m => m.SendCollectionAsync(requestInfo, It.IsAny<ParsableFactory<DummyModel>>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await adapter.SendCollectionAsync(requestInfo, DummyParsableFactory.CreateFromDiscriminatorValue);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When calling SendPrimitiveAsync, Then call inner adapter and return result")]
    public async Task SendPrimitiveAsync_GivenLunoTelemetryAdapter_WhenCallingSendPrimitiveAsync_ThenCallInnerAdapterAndReturnResult()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);
        var requestInfo = new RequestInformation();
        requestInfo.AddRequestOptions(new IRequestOption[] { new LunoTelemetryOptions("TestOperation") });

        innerMock.Setup(m => m.SendPrimitiveAsync<int>(requestInfo, It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        // Act
        var result = await adapter.SendPrimitiveAsync<int>(requestInfo);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When calling SendPrimitiveCollectionAsync, Then call inner adapter and return result")]
    public async Task SendPrimitiveCollectionAsync_GivenLunoTelemetryAdapter_WhenCallingSendPrimitiveCollectionAsync_ThenCallInnerAdapterAndReturnResult()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);
        var requestInfo = new RequestInformation();
        requestInfo.AddRequestOptions(new IRequestOption[] { new LunoTelemetryOptions("TestOperation") });
        var expectedResult = new List<int> { 42 };

        innerMock.Setup(m => m.SendPrimitiveCollectionAsync<int>(requestInfo, It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await adapter.SendPrimitiveCollectionAsync<int>(requestInfo);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When calling SendNoContentAsync, Then call inner adapter and return true")]
    public async Task SendNoContentAsync_GivenLunoTelemetryAdapter_WhenCallingSendNoContentAsync_ThenCallInnerAdapterAndReturnTrue()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);
        var requestInfo = new RequestInformation();
        requestInfo.AddRequestOptions(new IRequestOption[] { new LunoTelemetryOptions("TestOperation") });

        innerMock.Setup(m => m.SendNoContentAsync(requestInfo, It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await adapter.SendNoContentAsync(requestInfo);

        // Assert
        innerMock.Verify(m => m.SendNoContentAsync(requestInfo, It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When accessing SerializationWriterFactory, Then delegate to inner adapter")]
    public void SerializationWriterFactory_GivenLunoTelemetryAdapter_WhenAccessingSerializationWriterFactory_ThenDelegateToInnerAdapter()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var factoryMock = new Mock<ISerializationWriterFactory>();
        innerMock.Setup(m => m.SerializationWriterFactory).Returns(factoryMock.Object);
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);

        // Act
        var result = adapter.SerializationWriterFactory;

        // Assert
        Assert.Equal(factoryMock.Object, result);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When getting or setting BaseUrl, Then delegate to inner adapter")]
    public void BaseUrl_GivenLunoTelemetryAdapter_WhenGettingOrSettingBaseUrl_ThenDelegateToInnerAdapter()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        innerMock.SetupProperty(m => m.BaseUrl, "https://api.luno.com");
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);

        // Act
        var result = adapter.BaseUrl;
        adapter.BaseUrl = "https://api.luno.com/v2";

        // Assert
        Assert.Equal("https://api.luno.com", result);
        Assert.Equal("https://api.luno.com/v2", innerMock.Object.BaseUrl);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When calling EnableBackingStore, Then delegate to inner adapter")]
    public void EnableBackingStore_GivenLunoTelemetryAdapter_WhenCallingEnableBackingStore_ThenDelegateToInnerAdapter()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);
        var factoryMock = new Mock<IBackingStoreFactory>();

        // Act
        adapter.EnableBackingStore(factoryMock.Object);

        // Assert
        innerMock.Verify(m => m.EnableBackingStore(factoryMock.Object), Times.Once);
    }

    [Fact(DisplayName = "Given LunoTelemetryAdapter, When calling ConvertToNativeRequestAsync, Then delegate to inner adapter")]
    public async Task ConvertToNativeRequestAsync_GivenLunoTelemetryAdapter_WhenCallingConvertToNativeRequestAsync_ThenDelegateToInnerAdapter()
    {
        // Arrange
        var innerMock = new Mock<IRequestAdapter>();
        var adapter = new LunoTelemetryAdapter(innerMock.Object, new LunoTelemetry(), NullLogger.Instance);
        var requestInfo = new RequestInformation();
        var expectedRequest = new HttpRequestMessage();

        innerMock.Setup(m => m.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRequest);

        // Act
        var result = await adapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);

        // Assert
        Assert.Equal(expectedRequest, result);
    }
}
