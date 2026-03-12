using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Infrastructure.ErrorHandling;

/// <summary>
/// A decorator for <see cref="IRequestAdapter"/> that intercepts <see cref="ApiException"/>
/// and translates specific HTTP status codes and Luno error codes into domain-specific exceptions.
/// </summary>
public class LunoErrorHandlingAdapter : IRequestAdapter
{
    private readonly IRequestAdapter _innerAdapter;

    /// <summary>
    /// Initializes a new instance of the <see cref="LunoErrorHandlingAdapter"/> class.
    /// </summary>
    /// <param name="innerAdapter">The inner request adapter to delegate calls to.</param>
    public LunoErrorHandlingAdapter(IRequestAdapter innerAdapter)
    {
        _innerAdapter = innerAdapter;
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
        var codeProp = ex.GetType().GetProperty("Code", BindingFlags.Public | BindingFlags.Instance);
        if (codeProp != null)
        {
            errorCode = codeProp.GetValue(ex) as string;
        }

        // Parse retry-after header if present
        int? retryAfter = null;
        if (ex.ResponseHeaders != null && ex.ResponseHeaders.TryGetValue("Retry-After", out var retryAfterValues))
        {
            var retryAfterStr = System.Linq.Enumerable.FirstOrDefault(retryAfterValues);
            if (!string.IsNullOrEmpty(retryAfterStr) && int.TryParse(retryAfterStr, out var parsedRetryAfter))
            {
                retryAfter = parsedRetryAfter;
            }
        }

        int statusCode = ex.ResponseStatusCode;

        // Route by ErrorCode if available
        if (!string.IsNullOrEmpty(errorCode))
        {
            switch (errorCode)
            {
                // Security
                case "ErrUnauthorised":
                case "ErrApiKeyRevoked":
                case "ErrIncorrectPin":
                    throw new LunoUnauthorizedException(ex.Message, errorCode, statusCode, ex);
                case "ErrInsufficientPerms":
                    throw new LunoForbiddenException(ex.Message, errorCode, statusCode, ex);

                // Rate Limit
                case "ErrTooManyRequests":
                case "ErrAddressCreateRateLimitReached":
                case "ErrActiveCryptoRequestExists":
                case "ErrMaxActiveFiatRequestsExists":
                    throw new LunoRateLimitException(ex.Message, errorCode, statusCode, retryAfter, ex);

                // Timeout
                case "ErrDeadlineExceeded":
                    throw new LunoTimeoutException(ex.Message, ex);

                // Internal
                case "ErrInternal":
                    throw new LunoApiExceptionStub(ex.Message, errorCode, statusCode, ex); // fallback mapping

                // Idempotency
                case "ErrDuplicateClientOrderID":
                case "ErrDuplicateClientMoveID":
                case "ErrDuplicateExternalID":
                    throw new LunoIdempotencyException(ex.Message, errorCode, statusCode, ex);

                // Account Policy
                case "ErrVerificationLevelTooLow":
                case "ErrUserNotVerifiedForCurrency":
                case "ErrTravelRule":
                case "ErrUpdateRequired":
                case "ErrUserBlockedForCancelWithdrawal":
                case "ErrWithdrawalBlocked":
                case "ErrAccountLimit":
                case "ErrNoAddressesAssigned":
                    throw new LunoAccountPolicyExceptionStub(ex.Message, errorCode, statusCode, ex); // Use stub to instantiate abstract

                // Market State
                case "ErrUnderMaintenance":
                case "ErrMarketUnavailable":
                case "ErrPostOnlyMode":
                case "ErrMarketNotAllowed":
                case "ErrCannotTradeWhileQuoteActive":
                    throw new LunoMarketStateExceptionStub(ex.Message, errorCode, statusCode, ex); // Use stub

                // Resource Not Found
                case "ErrNotFound":
                case "ErrAccountNotFound":
                case "ErrBeneficiaryNotFound":
                case "ErrOrderNotFound":
                case "ErrWithdrawalNotFound":
                case "ErrFundsMoveNotFound":
                    throw new LunoResourceNotFoundException(ex.Message, errorCode, statusCode, ex);

                // Insufficient Funds
                case "ErrInsufficientFunds":
                case "ErrInsufficientBalance":
                    throw new LunoInsufficientFundsException(ex.Message, errorCode, statusCode, ex);

                // Order Rejected
                case "ErrAmountTooSmall":
                case "ErrAmountTooBig":
                case "ErrPriceTooHigh":
                case "ErrPriceTooLow":
                case "ErrVolumeTooLow":
                case "ErrVolumeTooHigh":
                case "ErrValueTooHigh":
                case "ErrInvalidPrice":
                case "ErrInvalidVolume":
                case "ErrInvalidOrderSide":
                case "ErrCannotStopUnknownOrNonPendingOrder":
                case "ErrNoTradesToInferStopDirection":
                case "ErrStopPriceTooHigh":
                case "ErrStopPriceTooLow":
                case "ErrInvalidStopDirection":
                case "ErrInvalidStopPrice":
                case "ErrNotEnoughLiquidity":
                case "ErrPostOnlyNotAllowed":
                case "ErrOrderCanceled":
                case "ErrPriceDenominationNotAllowed":
                case "ErrVolumeDenominationNotAllowed":
                    throw new LunoOrderRejectedException(ex.Message, errorCode, statusCode, ex);

                // Validation
                case "ErrInvalidParameters":
                case "ErrInvalidArguments":
                case "ErrInvalidAccount":
                case "ErrInvalidAccountID":
                case "ErrInvalidCurrency":
                case "ErrInvalidAmount":
                case "ErrInvalidDetails":
                case "ErrInvalidMarketPair":
                case "ErrInvalidClientOrderId":
                case "ErrInvalidOrderRef":
                case "ErrInvalidRequestType":
                case "ErrInvalidSourceAccount":
                case "ErrInvalidBranchCode":
                case "ErrInvalidAccountNumber":
                case "ErrAccountsNotDifferent":
                case "ErrAddressLimitReached":
                case "ErrBlockedSendsCurrency":
                case "ErrCounterDenominationNotAllowed":
                case "ErrCreditAccountNotTransactional":
                case "ErrCustomRefNotAllowed":
                case "ErrDebitAccountNotTransactional":
                case "ErrDescriptionTooLong":
                case "ErrDifferentCurrencies":
                case "ErrDisallowedTarget":
                case "ErrERC20AddressAlreadyAssigned":
                case "ErrERC20AssignNonDefault":
                case "ErrIncompatibleBeneficiary":
                case "ErrRejectedBeneficiary":
                case "ErrRequestTypeDoesNotSupportFastWithdrawals":
                case "ErrTooManyRowsRequested":
                case "ErrInvalidBaseVolume":
                case "ErrInvalidCounterVolume":
                case "ErrLimitOutOfRange":
                    throw new LunoValidationException(ex.Message, errorCode, statusCode, ex);
            }
        }

        // Fallback to Status Code checks for non-Luno specific errors
        if (statusCode == 401)
        {
            throw new LunoUnauthorizedException("Invalid API credentials. Verify your API Key ID and Secret.", errorCode, statusCode, ex);
        }
        if (statusCode == 403)
        {
            throw new LunoForbiddenException("The provided API credentials do not have permission to access this resource.", errorCode, statusCode, ex);
        }
        if (statusCode == 429)
        {
            throw new LunoRateLimitException("Too Many Requests.", errorCode, statusCode, retryAfter, ex);
        }
        if (statusCode == 404)
        {
            throw new LunoResourceNotFoundException("Resource not found.", errorCode, statusCode, ex);
        }
        if (statusCode == 409)
        {
            throw new LunoIdempotencyException("Conflict error.", errorCode, statusCode, ex);
        }
        if (statusCode == 408 || statusCode == 504)
        {
            throw new LunoTimeoutException("Request timed out.", ex);
        }
    }

    private class LunoApiExceptionStub : LunoApiException
    {
        public LunoApiExceptionStub(string message, string? errorCode, int? statusCode, Exception? innerException = null)
            : base(message, errorCode, statusCode, innerException) { }
    }

    private class LunoAccountPolicyExceptionStub : LunoAccountPolicyException
    {
        public LunoAccountPolicyExceptionStub(string message, string? errorCode, int? statusCode, Exception? innerException = null)
            : base(message, errorCode, statusCode, innerException) { }
    }

    private class LunoMarketStateExceptionStub : LunoMarketStateException
    {
        public LunoMarketStateExceptionStub(string message, string? errorCode, int? statusCode, Exception? innerException = null)
            : base(message, errorCode, statusCode, innerException) { }
    }
}
