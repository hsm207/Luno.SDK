using System.Globalization;
using Luno.SDK.Infrastructure.Account;
using Luno.SDK.Infrastructure.Generated.Models;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Account;

public class AccountMapperTests
{
    [Fact(DisplayName = "Given valid AccountBalance, When mapping to domain, Then use InvariantCulture to parse safely")]
    public void MapToDomain_GivenValidAccountBalance_WhenMapping_ThenUseInvariantCulture()
    {
        // Arrange
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            // Set culture to one that uses ',' for decimals, e.g., German
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var dto = new AccountBalance
            {
                AccountId = "123",
                Asset = "XBT",
                Balance = "1000.50",
                Reserved = "10.25",
                Unconfirmed = "5.75",
                Name = "My Wallet"
            };

            // Act
            var domain = dto.ToDomain();

            // Assert
            Assert.Equal("123", domain.AccountId);
            Assert.Equal("XBT", domain.Asset);
            Assert.Equal(1000.50m, domain.Available);
            Assert.Equal(10.25m, domain.Reserved);
            Assert.Equal(5.75m, domain.Unconfirmed);
            Assert.Equal("My Wallet", domain.Name);
            Assert.Equal(1010.75m, domain.Total); // 1000.50 + 10.25
        }
        finally
        {
            // Restore culture
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact(DisplayName = "Given null fields in AccountBalance, When mapping to domain, Then assign defaults or zero")]
    public void MapToDomain_GivenNullFields_WhenMapping_ThenAssignDefaults()
    {
        // Arrange
        var dto = new AccountBalance();

        // Act
        var domain = dto.ToDomain();

        // Assert
        Assert.Equal(string.Empty, domain.AccountId);
        Assert.Equal(string.Empty, domain.Asset);
        Assert.Equal(0m, domain.Available);
        Assert.Equal(0m, domain.Reserved);
        Assert.Equal(0m, domain.Unconfirmed);
        Assert.Equal(string.Empty, domain.Name);
        Assert.Equal(0m, domain.Total);
    }
}
