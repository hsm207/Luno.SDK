using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Luno.SDK.Infrastructure.ErrorHandling;

/// <summary>
/// A decorator for <see cref="IRequestAdapter"/> that intercepts <see cref="ApiException"/>
/// and translates specific HTTP status codes and Luno error codes into domain-specific exceptions.
/// </summary>
public class LunoErrorHandlingAdapter : IRequestAdapter
{
    private readonly IRequestAdapter _innerAdapter;

    private static readonly Dictionary<string, Func<string, string?, int?, int?, Exception?, Exception>> _errorCodeMap = new()
    {
        // Security
        { "ErrUnauthorised", (m, e, s, r, i) => new LunoUnauthorizedException(m, e, s, i) },
        { "ErrApiKeyRevoked", (m, e, s, r, i) => new LunoUnauthorizedException(m, e, s, i) },
        { "ErrIncorrectPin", (m, e, s, r, i) => new LunoUnauthorizedException(m, e, s, i) },
        { "ErrInsufficientPerms", (m, e, s, r, i) => new LunoForbiddenException(m, e, s, i) },

        // Rate Limit
        { "ErrTooManyRequests", (m, e, s, r, i) => new LunoRateLimitException(m, e, s, r, i) },
        { "ErrAddressCreateRateLimitReached", (m, e, s, r, i) => new LunoRateLimitException(m, e, s, r, i) },
        { "ErrActiveCryptoRequestExists", (m, e, s, r, i) => new LunoRateLimitException(m, e, s, r, i) },
        { "ErrMaxActiveFiatRequestsExists", (m, e, s, r, i) => new LunoRateLimitException(m, e, s, r, i) },

        // Timeout
        { "ErrDeadlineExceeded", (m, e, s, r, i) => new LunoTimeoutException(m, e, s, i) },

        // Internal
        { "ErrInternal", (m, e, s, r, i) => new LunoApiException(m, e, s, i) },

        // Idempotency
        { "ErrDuplicateClientOrderID", (m, e, s, r, i) => new LunoIdempotencyException(m, e, s, i) },
        { "ErrDuplicateClientMoveID", (m, e, s, r, i) => new LunoIdempotencyException(m, e, s, i) },
        { "ErrDuplicateExternalID", (m, e, s, r, i) => new LunoIdempotencyException(m, e, s, i) },

        // Account Policy
        { "ErrVerificationLevelTooLow", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },
        { "ErrUserNotVerifiedForCurrency", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },
        { "ErrTravelRule", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },
        { "ErrUpdateRequired", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },
        { "ErrUserBlockedForCancelWithdrawal", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },
        { "ErrWithdrawalBlocked", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },
        { "ErrAccountLimit", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },
        { "ErrNoAddressesAssigned", (m, e, s, r, i) => new LunoAccountPolicyException(m, e, s, i) },

        // Market State
        { "ErrUnderMaintenance", (m, e, s, r, i) => new LunoMarketStateException(m, e, s, i) },
        { "ErrMarketUnavailable", (m, e, s, r, i) => new LunoMarketStateException(m, e, s, i) },
        { "ErrPostOnlyMode", (m, e, s, r, i) => new LunoMarketStateException(m, e, s, i) },
        { "ErrMarketNotAllowed", (m, e, s, r, i) => new LunoMarketStateException(m, e, s, i) },
        { "ErrCannotTradeWhileQuoteActive", (m, e, s, r, i) => new LunoMarketStateException(m, e, s, i) },

        // Resource Not Found
        { "ErrNotFound", (m, e, s, r, i) => new LunoResourceNotFoundException(m, e, s, i) },
        { "ErrAccountNotFound", (m, e, s, r, i) => new LunoResourceNotFoundException(m, e, s, i) },
        { "ErrBeneficiaryNotFound", (m, e, s, r, i) => new LunoResourceNotFoundException(m, e, s, i) },
        { "ErrOrderNotFound", (m, e, s, r, i) => new LunoResourceNotFoundException(m, e, s, i) },
        { "ErrWithdrawalNotFound", (m, e, s, r, i) => new LunoResourceNotFoundException(m, e, s, i) },
        { "ErrFundsMoveNotFound", (m, e, s, r, i) => new LunoResourceNotFoundException(m, e, s, i) },

        // Insufficient Funds
        { "ErrInsufficientFunds", (m, e, s, r, i) => new LunoInsufficientFundsException(m, e, s, i) },
        { "ErrInsufficientBalance", (m, e, s, r, i) => new LunoInsufficientFundsException(m, e, s, i) },

        // Order Rejected
        { "ErrAmountTooSmall", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrAmountTooBig", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrPriceTooHigh", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrPriceTooLow", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrVolumeTooLow", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrVolumeTooHigh", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrValueTooHigh", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrInvalidPrice", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrInvalidVolume", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrInvalidOrderSide", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrCannotStopUnknownOrNonPendingOrder", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrNoTradesToInferStopDirection", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrStopPriceTooHigh", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrStopPriceTooLow", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrInvalidStopDirection", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrInvalidStopPrice", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrNotEnoughLiquidity", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrPostOnlyNotAllowed", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrOrderCanceled", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrPriceDenominationNotAllowed", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },
        { "ErrVolumeDenominationNotAllowed", (m, e, s, r, i) => new LunoOrderRejectedException(m, e, s, i) },

        // Validation
        { "ErrInvalidParameters", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidArguments", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidAccount", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidAccountID", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidCurrency", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidAmount", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidDetails", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidMarketPair", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidClientOrderId", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidOrderRef", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidRequestType", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidSourceAccount", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidBranchCode", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidAccountNumber", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrAccountsNotDifferent", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrAddressLimitReached", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrBlockedSendsCurrency", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrCounterDenominationNotAllowed", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrCreditAccountNotTransactional", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrCustomRefNotAllowed", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrDebitAccountNotTransactional", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrDescriptionTooLong", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrDifferentCurrencies", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrDisallowedTarget", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrERC20AddressAlreadyAssigned", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrERC20AssignNonDefault", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrIncompatibleBeneficiary", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrRejectedBeneficiary", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrRequestTypeDoesNotSupportFastWithdrawals", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrTooManyRowsRequested", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidBaseVolume", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrInvalidCounterVolume", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
        { "ErrLimitOutOfRange", (m, e, s, r, i) => new LunoValidationException(m, e, s, i) },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoErrorHandlingAdapter"/> class.
    /// </summary>
    /// <param name="innerAdapter">The inner request adapter to delegate calls to.</param>
    public LunoErrorHandlingAdapter(IRequestAdapter innerAdapter)
    {
        _innerAdapter = innerAdapter ?? throw new ArgumentNullException(nameof(innerAdapter));
    }

    /// <inheritdoc />
    public string? BaseUrl
    {
        get => _innerAdapter.BaseUrl;
        set => _innerAdapter.BaseUrl = value;
    }

    /// <inheritdoc />
    public ISerializationWriterFactory SerializationWriterFactory => _innerAdapter.SerializationWriterFactory;

    /// <inheritdoc />
    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory) =>
        _innerAdapter.EnableBackingStore(backingStoreFactory);

    /// <inheritdoc />
    public async Task<ModelType?> SendAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        try
        {
            return await _innerAdapter.SendAsync(requestInfo, factory, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        try
        {
            return await _innerAdapter.SendCollectionAsync(requestInfo, factory, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T?> SendPrimitiveAsync<T>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _innerAdapter.SendPrimitiveAsync<T>(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>?> SendPrimitiveCollectionAsync<T>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _innerAdapter.SendPrimitiveCollectionAsync<T>(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendNoContentAsync(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _innerAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException ex)
        {
            HandleException(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<T?> ConvertToNativeRequestAsync<T>(
        RequestInformation requestInfo,
        CancellationToken cancellationToken = default)
    {
        return _innerAdapter.ConvertToNativeRequestAsync<T>(requestInfo, cancellationToken);
    }

    private void HandleException(ApiException ex)
    {
        string? errorCode = null;

        // Try to get "Code" property via reflection if the specific generated ApiException contains it
        // The Luno OpenAPI spec is inconsistent. Some endpoints define a specific error schema
        // (resulting in a generated exception class with a 'Code' property), while others do not
        // (resulting in a generic ApiException without it). We use reflection to safely access
        // 'Code' if it exists on the actual runtime exception type.
        var codeProp = ex.GetType().GetProperty("Code", BindingFlags.Public | BindingFlags.Instance);
        if (codeProp != null)
        {
            errorCode = codeProp.GetValue(ex) as string;
        }

        // Parse retry-after header if present
        int? retryAfter = null;
        if (ex.ResponseHeaders != null && ex.ResponseHeaders.TryGetValue("Retry-After", out var retryAfterValues))
        {
            var retryAfterStr = retryAfterValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(retryAfterStr) && int.TryParse(retryAfterStr, out var parsedRetryAfter))
            {
                retryAfter = parsedRetryAfter;
            }
        }

        int statusCode = ex.ResponseStatusCode;

        // Route by ErrorCode if available
        if (!string.IsNullOrEmpty(errorCode) && _errorCodeMap.TryGetValue(errorCode, out var factory))
        {
            throw factory(ex.Message, errorCode, statusCode, retryAfter, ex);
        }

        // Fallback to Status Code checks for non-Luno specific errors
        switch (statusCode)
        {
            case 401:
                throw new LunoUnauthorizedException("Invalid API credentials. Verify your API Key ID and Secret.", errorCode, statusCode, ex);
            case 403:
                throw new LunoForbiddenException("The provided API credentials do not have permission to access this resource.", errorCode, statusCode, ex);
            case 429:
                throw new LunoRateLimitException("Too Many Requests.", errorCode, statusCode, retryAfter, ex);
            case 404:
                throw new LunoResourceNotFoundException("Resource not found.", errorCode, statusCode, ex);
            case 409:
                throw new LunoIdempotencyException("Conflict error.", errorCode, statusCode, ex);
            case 408:
            case 504:
                throw new LunoTimeoutException("Request timed out.", errorCode, statusCode, ex);
        }

        // Fallback for unmapped errors (e.g. 500 Internal Server Error, or unknown 400s)
        // ensures strict adherence to RFC004 Goal: Standardize on LunoException.
        throw new LunoApiException($"An unexpected API error occurred (HTTP {statusCode})", errorCode, statusCode, ex);
    }
}
