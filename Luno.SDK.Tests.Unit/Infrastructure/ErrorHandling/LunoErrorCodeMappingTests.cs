using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Luno.SDK;
using Luno.SDK.Infrastructure.ErrorHandling;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.ErrorHandling;

/// <summary>
/// Verifies the high-fidelity mapping of Luno API error codes to semantic domain exceptions.
/// </summary>
public class LunoErrorCodeMappingTests
{
    private class TestApiException : ApiException
    {
        public string? Code { get; set; }
    }

    [Theory(DisplayName = "Given a Luno error code, When HandleException is called, Then throw the corresponding semantic exception")]
    [InlineData("ErrUnauthorised", typeof(LunoUnauthorizedException))]
    [InlineData("ErrApiKeyRevoked", typeof(LunoUnauthorizedException))]
    [InlineData("ErrIncorrectPin", typeof(LunoUnauthorizedException))]
    [InlineData("ErrInsufficientPerms", typeof(LunoForbiddenException))]
    [InlineData("ErrTooManyRequests", typeof(LunoRateLimitException))]
    [InlineData("ErrAddressCreateRateLimitReached", typeof(LunoRateLimitException))]
    [InlineData("ErrActiveCryptoRequestExists", typeof(LunoRateLimitException))]
    [InlineData("ErrMaxActiveFiatRequestsExists", typeof(LunoRateLimitException))]
    [InlineData("ErrDeadlineExceeded", typeof(LunoTimeoutException))]
    [InlineData("ErrInternal", typeof(LunoApiException))]
    [InlineData("ErrDuplicateClientOrderID", typeof(LunoIdempotencyException))]
    [InlineData("ErrDuplicateClientMoveID", typeof(LunoIdempotencyException))]
    [InlineData("ErrDuplicateExternalID", typeof(LunoIdempotencyException))]
    [InlineData("ErrVerificationLevelTooLow", typeof(LunoAccountPolicyException))]
    [InlineData("ErrUserNotVerifiedForCurrency", typeof(LunoAccountPolicyException))]
    [InlineData("ErrTravelRule", typeof(LunoAccountPolicyException))]
    [InlineData("ErrUpdateRequired", typeof(LunoAccountPolicyException))]
    [InlineData("ErrUserBlockedForCancelWithdrawal", typeof(LunoAccountPolicyException))]
    [InlineData("ErrWithdrawalBlocked", typeof(LunoAccountPolicyException))]
    [InlineData("ErrAccountLimit", typeof(LunoAccountPolicyException))]
    [InlineData("ErrNoAddressesAssigned", typeof(LunoAccountPolicyException))]
    [InlineData("ErrUnderMaintenance", typeof(LunoMarketStateException))]
    [InlineData("ErrMarketUnavailable", typeof(LunoMarketStateException))]
    [InlineData("ErrPostOnlyMode", typeof(LunoMarketStateException))]
    [InlineData("ErrMarketNotAllowed", typeof(LunoMarketStateException))]
    [InlineData("ErrCannotTradeWhileQuoteActive", typeof(LunoMarketStateException))]
    [InlineData("ErrNotFound", typeof(LunoResourceNotFoundException))]
    [InlineData("ErrAccountNotFound", typeof(LunoResourceNotFoundException))]
    [InlineData("ErrBeneficiaryNotFound", typeof(LunoResourceNotFoundException))]
    [InlineData("ErrOrderNotFound", typeof(LunoResourceNotFoundException))]
    [InlineData("ErrWithdrawalNotFound", typeof(LunoResourceNotFoundException))]
    [InlineData("ErrFundsMoveNotFound", typeof(LunoResourceNotFoundException))]
    [InlineData("ErrInsufficientFunds", typeof(LunoInsufficientFundsException))]
    [InlineData("ErrInsufficientBalance", typeof(LunoInsufficientFundsException))]
    [InlineData("ErrAmountTooSmall", typeof(LunoOrderRejectedException))]
    [InlineData("ErrAmountTooBig", typeof(LunoOrderRejectedException))]
    [InlineData("ErrPriceTooHigh", typeof(LunoOrderRejectedException))]
    [InlineData("ErrPriceTooLow", typeof(LunoOrderRejectedException))]
    [InlineData("ErrVolumeTooLow", typeof(LunoOrderRejectedException))]
    [InlineData("ErrVolumeTooHigh", typeof(LunoOrderRejectedException))]
    [InlineData("ErrValueTooHigh", typeof(LunoOrderRejectedException))]
    [InlineData("ErrInvalidPrice", typeof(LunoOrderRejectedException))]
    [InlineData("ErrInvalidVolume", typeof(LunoOrderRejectedException))]
    [InlineData("ErrInvalidOrderSide", typeof(LunoOrderRejectedException))]
    [InlineData("ErrCannotStopUnknownOrNonPendingOrder", typeof(LunoOrderRejectedException))]
    [InlineData("ErrNoTradesToInferStopDirection", typeof(LunoOrderRejectedException))]
    [InlineData("ErrStopPriceTooHigh", typeof(LunoOrderRejectedException))]
    [InlineData("ErrStopPriceTooLow", typeof(LunoOrderRejectedException))]
    [InlineData("ErrInvalidStopDirection", typeof(LunoOrderRejectedException))]
    [InlineData("ErrInvalidStopPrice", typeof(LunoOrderRejectedException))]
    [InlineData("ErrNotEnoughLiquidity", typeof(LunoOrderRejectedException))]
    [InlineData("ErrPostOnlyNotAllowed", typeof(LunoOrderRejectedException))]
    [InlineData("ErrOrderCanceled", typeof(LunoOrderRejectedException))]
    [InlineData("ErrPriceDenominationNotAllowed", typeof(LunoOrderRejectedException))]
    [InlineData("ErrVolumeDenominationNotAllowed", typeof(LunoOrderRejectedException))]
    [InlineData("ErrInvalidParameters", typeof(LunoValidationException))]
    [InlineData("ErrInvalidArguments", typeof(LunoValidationException))]
    [InlineData("ErrInvalidAccount", typeof(LunoValidationException))]
    [InlineData("ErrInvalidAccountID", typeof(LunoValidationException))]
    [InlineData("ErrInvalidCurrency", typeof(LunoValidationException))]
    [InlineData("ErrInvalidAmount", typeof(LunoValidationException))]
    [InlineData("ErrInvalidDetails", typeof(LunoValidationException))]
    [InlineData("ErrInvalidMarketPair", typeof(LunoValidationException))]
    [InlineData("ErrInvalidClientOrderId", typeof(LunoValidationException))]
    [InlineData("ErrInvalidOrderRef", typeof(LunoValidationException))]
    [InlineData("ErrInvalidRequestType", typeof(LunoValidationException))]
    [InlineData("ErrInvalidSourceAccount", typeof(LunoValidationException))]
    [InlineData("ErrInvalidBranchCode", typeof(LunoValidationException))]
    [InlineData("ErrInvalidAccountNumber", typeof(LunoValidationException))]
    [InlineData("ErrAccountsNotDifferent", typeof(LunoValidationException))]
    [InlineData("ErrAddressLimitReached", typeof(LunoValidationException))]
    [InlineData("ErrBlockedSendsCurrency", typeof(LunoValidationException))]
    [InlineData("ErrCounterDenominationNotAllowed", typeof(LunoValidationException))]
    [InlineData("ErrCreditAccountNotTransactional", typeof(LunoValidationException))]
    [InlineData("ErrCustomRefNotAllowed", typeof(LunoValidationException))]
    [InlineData("ErrDebitAccountNotTransactional", typeof(LunoValidationException))]
    [InlineData("ErrDescriptionTooLong", typeof(LunoValidationException))]
    [InlineData("ErrDifferentCurrencies", typeof(LunoValidationException))]
    [InlineData("ErrDisallowedTarget", typeof(LunoValidationException))]
    [InlineData("ErrERC20AddressAlreadyAssigned", typeof(LunoValidationException))]
    [InlineData("ErrERC20AssignNonDefault", typeof(LunoValidationException))]
    [InlineData("ErrIncompatibleBeneficiary", typeof(LunoValidationException))]
    [InlineData("ErrRejectedBeneficiary", typeof(LunoValidationException))]
    [InlineData("ErrRequestTypeDoesNotSupportFastWithdrawals", typeof(LunoValidationException))]
    [InlineData("ErrTooManyRowsRequested", typeof(LunoValidationException))]
    [InlineData("ErrInvalidBaseVolume", typeof(LunoValidationException))]
    [InlineData("ErrInvalidCounterVolume", typeof(LunoValidationException))]
    [InlineData("ErrLimitOutOfRange", typeof(LunoValidationException))]
    public async Task HandleException_MapsErrorCodeToCorrectException(string errorCode, Type expectedExceptionType)
    {
        // Arrange
        var apiEx = new TestApiException { Code = errorCode, ResponseStatusCode = 400 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        var exception = await Assert.ThrowsAsync(expectedExceptionType, () => errorAdapter.SendNoContentAsync(requestInfo));
        
        if (exception is LunoApiException lunoEx)
        {
            Assert.Equal(errorCode, lunoEx.ErrorCode);
        }
    }

    [Fact(DisplayName = "Given a rate limit error with Retry-After header, When HandleException is called, Then populate RetryAfter property")]
    public async Task HandleException_WithRetryAfterHeader_PopulatesRateLimitException()
    {
        // Arrange
        var apiEx = new TestApiException 
        { 
            Code = "ErrTooManyRequests", 
            ResponseStatusCode = 429,
            ResponseHeaders = new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { "60" } }
            }
        };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoRateLimitException>(() => errorAdapter.SendNoContentAsync(requestInfo));
        Assert.Equal(60, ex.RetryAfter);
    }

    [Theory(DisplayName = "Given an HTTP status code without a Luno error code, When HandleException is called, Then fall back to the corresponding semantic exception")]
    [InlineData(401, typeof(LunoUnauthorizedException))]
    [InlineData(403, typeof(LunoForbiddenException))]
    [InlineData(429, typeof(LunoRateLimitException))]
    [InlineData(404, typeof(LunoResourceNotFoundException))]
    [InlineData(409, typeof(LunoIdempotencyException))]
    [InlineData(408, typeof(LunoTimeoutException))]
    [InlineData(504, typeof(LunoTimeoutException))]
    public async Task HandleException_FallsBackToStatusCodeWhenErrorCodeIsMissing(int statusCode, Type expectedExceptionType)
    {
        // Arrange
        var apiEx = new TestApiException { Code = null, ResponseStatusCode = statusCode };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        await Assert.ThrowsAsync(expectedExceptionType, () => errorAdapter.SendNoContentAsync(requestInfo));
    }

    [Fact(DisplayName = "Given an ApiException, When SendAsync overloads are called, Then HandleException is triggered for all of them")]
    public async Task HandleException_TriggeredByAllSendOverloads()
    {
        // Arrange
        var apiEx = new TestApiException { Code = "ErrInternal", ResponseStatusCode = 500 };
        var innerAdapter = new StubRequestAdapter(apiEx);
        var errorAdapter = new LunoErrorHandlingAdapter(innerAdapter);
        var requestInfo = new RequestInformation();

        // Act & Assert
        await Assert.ThrowsAsync<LunoApiException>(() => errorAdapter.SendAsync<Luno.SDK.Infrastructure.Generated.Models.AccountBalance>(requestInfo, (n) => null!));
        await Assert.ThrowsAsync<LunoApiException>(() => errorAdapter.SendCollectionAsync<Luno.SDK.Infrastructure.Generated.Models.AccountBalance>(requestInfo, (n) => null!));
        await Assert.ThrowsAsync<LunoApiException>(() => errorAdapter.SendPrimitiveAsync<int>(requestInfo));
        await Assert.ThrowsAsync<LunoApiException>(() => errorAdapter.SendPrimitiveCollectionAsync<int>(requestInfo));
    }
}
