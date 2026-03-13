using System;

namespace Luno.SDK;

/// <summary>
/// Exception representing account policy, KYC, or permission issues.
/// </summary>
/// <remarks>
/// Primary Luno error codes: ErrVerificationLevelTooLow, ErrUserNotVerifiedForCurrency, ErrTravelRule, ErrUpdateRequired, ErrUserBlockedForCancelWithdrawal, ErrWithdrawalBlocked, ErrAccountLimit, ErrNoAddressesAssigned.
/// </remarks>
[Serializable]
public class LunoAccountPolicyException : LunoApiException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class.
    /// </summary>
    public LunoAccountPolicyException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LunoAccountPolicyException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoAccountPolicyException(string message, Exception? innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoAccountPolicyException"/> class with metadata.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorCode">The raw error code string from Luno.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The inner exception.</param>
    public LunoAccountPolicyException(string message, string? errorCode, int? statusCode, Exception? innerException = null)
        : base(message, errorCode, statusCode, innerException) { }
}
