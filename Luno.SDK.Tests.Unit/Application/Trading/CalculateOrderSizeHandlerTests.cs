using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Market;
using Luno.SDK.Application.Trading;
using Luno.SDK.Telemetry;
using Moq;
using Xunit;

namespace Luno.SDK.Tests.Unit.Application.Trading;

public class CalculateOrderSizeHandlerTests
{
    private readonly Mock<ILunoMarketOperations> _marketMock;
    private readonly Mock<ILunoTelemetry> _telemetryMock;
    private readonly CalculateOrderSizeHandler _handler;

    public CalculateOrderSizeHandlerTests()
    {
        _marketMock = new Mock<ILunoMarketOperations>();
        _telemetryMock = new Mock<ILunoTelemetry>();
        var meter = new System.Diagnostics.Metrics.Meter("Luno.SDK.Tests");
        _telemetryMock.SetupGet(t => t.Meter).Returns(meter);
        
        _handler = new CalculateOrderSizeHandler(_marketMock.Object, _telemetryMock.Object);
    }

    private void SetupMarket(string pair, decimal minVol, decimal maxVol, decimal maxPrice, int volScale, int priceScale, MarketStatus status = MarketStatus.Active)
    {
        _marketMock.Setup(m => m.FetchMarketsAsync(It.IsAny<string[]?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MarketInfo>
            {
                new MarketInfo 
                { 
                    Pair = pair, 
                    BaseCurrency = "XBT", 
                    CounterCurrency = "MYR", 
                    MinVolume = minVol, 
                    MaxVolume = maxVol, 
                    MinPrice = 0.01m, 
                    MaxPrice = maxPrice, 
                    VolumeScale = volScale, 
                    PriceScale = priceScale, 
                    Status = status, 
                    FeeScale = 2 
                }
            });
    }

    private void SetupTicker(string pair, decimal ask, decimal bid)
    {
        _marketMock.Setup(m => m.FetchTickerAsync(pair, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ticker(pair, ask, bid, Math.Round((ask+bid)/2, 2), 1000m, MarketStatus.Active, DateTimeOffset.UtcNow));
    }

    [Fact(DisplayName = "Given Market-Relative Buy, When calculated, Then select Ask and round DOWN to save quote")]
    public async Task Handle_MarketBuy_RoundsPriceDown()
    {
        // Arrange
        SetupMarket("XBTMYR", 0.0001m, 100m, 1000000m, 6, 2);
        SetupTicker("XBTMYR", 250000.005m, 249000.00m); // Ask has 3 decimals, scale is 2
        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Buy, TradingAmount.InQuote(100m));

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(250000.00m, result.Price); // Rounded down
        Assert.Equal(0.000400m, result.Volume); // 100 / 250000 = 0.0004
    }

    [Fact(DisplayName = "Given Market-Relative Sell, When calculated, Then select Bid and round UP to ensure floor")]
    public async Task Handle_MarketSell_RoundsPriceUp()
    {
        // Arrange
        SetupMarket("XBTMYR", 0.0001m, 100m, 1000000m, 6, 2);
        SetupTicker("XBTMYR", 250000.00m, 248999.991m); // Bid has 3 decimals, scale is 2
        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Sell, TradingAmount.InQuote(100m));

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(249000.00m, result.Price); // Rounded up
        Assert.Equal(0.000401m, result.Volume); // 100 / 249000 = 0.0004016 -> Flored to 0.000401
    }

    [Fact(DisplayName = "Given Target-Fixed Order, When calculated, Then ignores Ticker and uses manual price")]
    public async Task Handle_FixedPrice_IgnoresTicker()
    {
        // Arrange
        SetupMarket("XBTMYR", 0.0001m, 100m, 1000000m, 6, 2);
        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Buy, TradingAmount.InQuote(100m), TradingPrice.InQuote(200000m));

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(200000.00m, result.Price);
        Assert.Equal(0.000500m, result.Volume);
        
        // Ensure ticker is not called
        _marketMock.Verify(m => m.FetchTickerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Given volume below minimum, When calculated, Then throws LunoValidationException")]
    public async Task Handle_VolumeBelowMin_ThrowsValidationException()
    {
        // Arrange
        SetupMarket("XBTMYR", 0.0005m, 100m, 1000000m, 6, 2);
        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Buy, TradingAmount.InQuote(50m), TradingPrice.InQuote(200000m)); // gives 0.00025

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => _handler.HandleAsync(query));
        Assert.Contains("minimum volume", ex.Message.ToLower());
    }

    [Fact(DisplayName = "Given a dead market, When calculating without AtPrice, Then throws InvalidPrice exception")]
    public async Task Handle_DeadMarket_ThrowsValidationException()
    {
        // Arrange
        SetupMarket("XBTMYR", 0.0005m, 100m, 1000000m, 6, 2);
        SetupTicker("XBTMYR", 0m, 0m); // Dead market
        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Buy, TradingAmount.InQuote(100m));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => _handler.HandleAsync(query));
        Assert.Contains("invalid price", ex.Message.ToLower());
    }

    [Fact(DisplayName = "Given a Spend in Base currency, When calculated, Then volume matches exactly floored spend")]
    public async Task Handle_SpendInBase_SetsExactVolume()
    {
        // Arrange
        SetupMarket("XBTMYR", 0.0001m, 100m, 1000000m, 6, 2);
        SetupTicker("XBTMYR", 250000.00m, 240000.00m);
        // Spend 0.1234567 BTC (more decimals than scale)
        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Buy, TradingAmount.InBase(0.1234567m));

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(250000.00m, result.Price);
        Assert.Equal(0.123456m, result.Volume); // Floored to 6 decimals
    }

    [Fact(DisplayName = "Given valid Buy Ask but dead Bid, When calculated, Then Buy succeeds")]
    public async Task Handle_SidedLiquidity_AllowsDeadOppositeSide()
    {
        // Arrange
        SetupMarket("XBTMYR", 0.0001m, 100m, 1000000m, 6, 2);
        SetupTicker("XBTMYR", ask: 250000.00m, bid: 0m); // Dead Bid
        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Buy, TradingAmount.InBase(0.1m));

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(250000.00m, result.Price);
    }

    [Fact(DisplayName = "Given negative TradingAmount, When created, Then throws ArgumentOutOfRangeException")]
    public void TradingAmount_Negative_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TradingAmount.InBase(-0.1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => TradingAmount.InQuote(-10m));
        Assert.Throws<ArgumentOutOfRangeException>(() => TradingAmount.InBase(0m));
    }
}
