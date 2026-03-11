using System.Globalization;
using Luno.SDK.Infrastructure.Account;
using Luno.SDK.Infrastructure.Generated.Models;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Account;

public class AccountMapperTests
{
    [Fact(DisplayName = "Given valid AccountBalance, When mapping to domain, Then use InvariantCulture to parse safely")]
    public void MapToDomain_ValidAccountBalance_UsesInvariantCulture()
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

    [Fact(DisplayName = "Given missing AccountId, When mapping to domain, Then throw LunoMappingException")]
    public void MapToDomain_MissingAccountId_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new AccountBalance
        {
            Asset = "XBT"
        };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => dto.ToDomain());
        Assert.Contains("Missing mandatory field: account_id", ex.Message);
        Assert.Equal(nameof(AccountBalance), ex.DtoType);
    }

    [Fact(DisplayName = "Given missing Asset, When mapping to domain, Then throw LunoMappingException")]
    public void MapToDomain_MissingAsset_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new AccountBalance
        {
            AccountId = "123"
        };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => dto.ToDomain());
        Assert.Contains("Missing mandatory field: asset", ex.Message);
        Assert.Equal(nameof(AccountBalance), ex.DtoType);
    }
}
