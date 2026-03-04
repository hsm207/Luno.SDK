using System.Globalization;
using Luno.SDK.Core.Account;
using Luno.SDK.Infrastructure.Generated.Models;

namespace Luno.SDK.Infrastructure.Account;

/// <summary>
/// Maps Kiota generated account models to core domain entities.
/// </summary>
public static class AccountMapper
{
    /// <summary>
    /// Maps a <see cref="AccountBalance"/> to a domain <see cref="Balance"/>.
    /// Uses InvariantCulture to ensure precise decimal parsing across different system locales.
    /// </summary>
    /// <param name="dto">The generated DTO from Kiota.</param>
    /// <returns>A domain <see cref="Balance"/>.</returns>
    public static Balance ToDomain(this AccountBalance dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Balance
        {
            AccountId = dto.AccountId ?? string.Empty,
            Asset = dto.Asset ?? string.Empty,
            Available = decimal.Parse(dto.Balance ?? "0", CultureInfo.InvariantCulture),
            Reserved = decimal.Parse(dto.Reserved ?? "0", CultureInfo.InvariantCulture),
            Unconfirmed = decimal.Parse(dto.Unconfirmed ?? "0", CultureInfo.InvariantCulture),
            Name = dto.Name ?? string.Empty
        };
    }
}
