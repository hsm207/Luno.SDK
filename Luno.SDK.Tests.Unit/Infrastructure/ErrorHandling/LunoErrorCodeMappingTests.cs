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
    [InlineData("ErrDeadlineExceeded", typeof(LunoTimeoutException))]
    [InlineData("ErrDuplicateClientOrderID", typeof(LunoIdempotencyException))]
    [InlineData("ErrVerificationLevelTooLow", typeof(LunoAccountPolicyException))]
    [InlineData("ErrUnderMaintenance", typeof(LunoMarketStateException))]
    [InlineData("ErrNotFound", typeof(LunoResourceNotFoundException))]
    [InlineData("ErrInsufficientFunds", typeof(LunoInsufficientFundsException))]
    [InlineData("ErrAmountTooSmall", typeof(LunoOrderRejectedException))]
    [InlineData("ErrInvalidParameters", typeof(LunoValidationException))]
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
}
