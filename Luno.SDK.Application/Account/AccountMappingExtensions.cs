using Luno.SDK.Account;

namespace Luno.SDK.Application.Account;

/// <summary>
/// Provides extension methods for mapping account domain entities to application-layer responses.
/// </summary>
internal static class AccountMappingExtensions
{
    /// <summary>
    /// Maps a <see cref="Balance"/> domain entity to an <see cref="AccountBalanceResponse"/> DTO.
    /// </summary>
    public static AccountBalanceResponse ToResponse(this Balance balance) => new(
        balance.AccountId,
        balance.Asset,
        balance.Available,
        balance.Reserved,
        balance.Unconfirmed,
        balance.Total,
        balance.Name
    );
}
