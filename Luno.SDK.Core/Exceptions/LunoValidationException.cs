using System;

namespace Luno.SDK;

/// <summary>
/// Exception thrown when request parameters fail validation.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrInvalidParameters, ErrInvalidArguments, ErrInvalidAccount, ErrInvalidAccountID, ErrInvalidCurrency, ErrInvalidAmount, ErrInvalidDetails, ErrInvalidMarketPair, ErrInvalidClientOrderId, ErrInvalidOrderRef, ErrInvalidRequestType, ErrInvalidSourceAccount, ErrInvalidBranchCode, ErrInvalidAccountNumber, ErrAccountsNotDifferent, ErrAddressLimitReached, ErrBlockedSendsCurrency, ErrCounterDenominationNotAllowed, ErrCreditAccountNotTransactional, ErrCustomRefNotAllowed, ErrDebitAccountNotTransactional, ErrDescriptionTooLong, ErrDifferentCurrencies, ErrDisallowedTarget, ErrERC20AddressAlreadyAssigned, ErrERC20AssignNonDefault, ErrIncompatibleBeneficiary, ErrRejectedBeneficiary, ErrRequestTypeDoesNotSupportFastWithdrawals, ErrTooManyRowsRequested, ErrInvalidBaseVolume, ErrInvalidCounterVolume, ErrLimitOutOfRange
/// </remarks>
[Serializable]
public class LunoValidationException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoValidationException"/> class.
    /// </summary>
    public LunoValidationException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoValidationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoValidationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoValidationException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoValidationException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoValidationException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
