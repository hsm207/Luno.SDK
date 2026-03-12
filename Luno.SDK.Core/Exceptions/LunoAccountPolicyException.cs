using System;

namespace Luno.SDK;

/// <summary>
/// The base exception for KYC, verification, and permission issues.
/// </summary>
/// <remarks>
/// Mapped Luno Error Codes: ErrVerificationLevelTooLow, ErrUserNotVerifiedForCurrency, ErrTravelRule, ErrUpdateRequired, ErrUserBlockedForCancelWithdrawal, ErrWithdrawalBlocked, ErrAccountLimit, ErrNoAddressesAssigned
/// </remarks>
[Serializable]
public abstract class LunoAccountPolicyException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class.
    /// </summary>
    protected LunoAccountPolicyException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected LunoAccountPolicyException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoAccountPolicyException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    protected LunoAccountPolicyException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
