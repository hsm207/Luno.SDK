namespace Luno.SDK.Application.Account;

/// <summary>
/// Represents the application-layer response containing balance data for a specific account.
/// </summary>
/// <param name="AccountId">ID of the account.</param>
/// <param name="Asset">Currency code for the asset held in this account.</param>
/// <param name="Available">The amount available to send or trade.</param>
/// <param name="Reserved">Amount locked by Luno and cannot be sent or traded. This could be due to open orders.</param>
/// <param name="Unconfirmed">Amount that is awaiting some sort of verification to be credited to this account. This could be an on-chain transaction that Luno is waiting for further block verifications to happen.</param>
/// <param name="Total">The calculated total amount (Available + Reserved).</param>
/// <param name="Name">The name set by the user upon creating the account.</param>
public record AccountBalanceResponse(
    string AccountId,
    string Asset,
    decimal Available,
    decimal Reserved,
    decimal Unconfirmed,
    decimal Total,
    string Name
);
