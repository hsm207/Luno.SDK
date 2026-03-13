using System;
using System.Collections.Generic;
using Luno.SDK;
using Xunit;

namespace Luno.SDK.Tests.Unit.Core.Exceptions;

/// <summary>
/// Contains lightweight unit tests for exception constructors to satisfy
/// architectural compliance and coverage mandates without using ExcludeFromCodeCoverage.
/// </summary>
public class LunoExceptionComplianceTests
{
    private readonly string _testMessage = "Test message";
    private readonly Exception _testInnerException = new Exception("Inner exception");

    // Explicitly test the abstract base classes by creating concrete test stubs
    private class TestLunoException : LunoException
    {
        public TestLunoException() : base() { }
        public TestLunoException(string message) : base(message) { }
        public TestLunoException(string message, Exception inner) : base(message, inner) { }
    }

    private class TestLunoApiException : LunoApiException
    {
        public TestLunoApiException() : base() { }
        public TestLunoApiException(string message) : base(message) { }
        public TestLunoApiException(string message, Exception inner) : base(message, inner) { }
        public TestLunoApiException(string message, string? errorCode, int? statusCode, Exception? inner = null)
            : base(message, errorCode, statusCode, inner) { }
    }

    private class TestLunoSecurityException : LunoSecurityException
    {
        public TestLunoSecurityException() : base() { }
        public TestLunoSecurityException(string message) : base(message) { }
        public TestLunoSecurityException(string message, Exception inner) : base(message, inner) { }
        public TestLunoSecurityException(string message, string? errorCode, int? statusCode, Exception? inner = null)
            : base(message, errorCode, statusCode, inner) { }
    }

    private class TestLunoDataException : LunoDataException
    {
        public TestLunoDataException() : base() { }
        public TestLunoDataException(string message) : base(message) { }
        public TestLunoDataException(string message, Exception inner) : base(message, inner) { }
    }

    private class TestLunoBusinessRuleException : LunoBusinessRuleException
    {
        public TestLunoBusinessRuleException() : base() { }
        public TestLunoBusinessRuleException(string message) : base(message) { }
        public TestLunoBusinessRuleException(string message, Exception inner) : base(message, inner) { }
        public TestLunoBusinessRuleException(string message, string? errorCode, int? statusCode, Exception? inner = null)
            : base(message, errorCode, statusCode, inner) { }
    }

    private class TestLunoAccountPolicyException : LunoAccountPolicyException
    {
        public TestLunoAccountPolicyException() : base() { }
        public TestLunoAccountPolicyException(string message) : base(message) { }
        public TestLunoAccountPolicyException(string message, Exception inner) : base(message, inner) { }
        public TestLunoAccountPolicyException(string message, string? errorCode, int? statusCode, Exception? inner = null)
            : base(message, errorCode, statusCode, inner) { }
    }

    private class TestLunoMarketStateException : LunoMarketStateException
    {
        public TestLunoMarketStateException() : base() { }
        public TestLunoMarketStateException(string message) : base(message) { }
        public TestLunoMarketStateException(string message, Exception inner) : base(message, inner) { }
        public TestLunoMarketStateException(string message, string? errorCode, int? statusCode, Exception? inner = null)
            : base(message, errorCode, statusCode, inner) { }
    }

    public static IEnumerable<object[]> ConcreteExceptionTypes()
    {
        yield return new object[] { typeof(LunoAuthenticationException) };
        yield return new object[] { typeof(LunoForbiddenException) };
        yield return new object[] { typeof(LunoUnauthorizedException) };
        yield return new object[] { typeof(LunoMappingException) };
        yield return new object[] { typeof(LunoRateLimitException) };
        yield return new object[] { typeof(LunoValidationException) };
        yield return new object[] { typeof(LunoIdempotencyException) };
        yield return new object[] { typeof(LunoOrderRejectedException) };
        yield return new object[] { typeof(LunoInsufficientFundsException) };
        yield return new object[] { typeof(LunoTimeoutException) };
        yield return new object[] { typeof(LunoResourceNotFoundException) };
        yield return new object[] { typeof(LunoClientException) };
    }

    [Theory(DisplayName = "Given an exception type, When constructed with parameterless constructor, Then inherit LunoException")]
    [MemberData(nameof(ConcreteExceptionTypes))]
    public void Constructor_Parameterless_InheritsLunoException(Type exceptionType)
    {
        // Act
        var ex = (Exception)Activator.CreateInstance(exceptionType)!;

        // Assert
        Assert.NotNull(ex);
        Assert.IsAssignableFrom<LunoException>(ex);
    }

    [Theory(DisplayName = "Given an exception type, When constructed with message, Then set message and inherit LunoException")]
    [MemberData(nameof(ConcreteExceptionTypes))]
    public void Constructor_Message_SetsMessage(Type exceptionType)
    {
        // Act
        var ex = (Exception)Activator.CreateInstance(exceptionType, _testMessage)!;

        // Assert
        Assert.NotNull(ex);
        Assert.IsAssignableFrom<LunoException>(ex);
        Assert.Equal(_testMessage, ex.Message);
    }

    [Theory(DisplayName = "Given an exception type, When constructed with message and inner exception, Then set both and inherit LunoException")]
    [MemberData(nameof(ConcreteExceptionTypes))]
    public void Constructor_MessageAndInnerException_SetsBoth(Type exceptionType)
    {
        // Act
        var ex = (Exception)Activator.CreateInstance(exceptionType, _testMessage, _testInnerException)!;

        // Assert
        Assert.NotNull(ex);
        Assert.IsAssignableFrom<LunoException>(ex);
        Assert.Equal(_testMessage, ex.Message);
        Assert.Equal(_testInnerException, ex.InnerException);
    }

    [Fact(DisplayName = "Given LunoMappingException, When constructed with dto type, Then set DtoType property")]
    public void LunoMappingException_DtoType_SetsDtoType()
    {
        // Act
        var ex = new LunoMappingException(_testMessage, "TestDto");

        // Assert
        Assert.Equal(_testMessage, ex.Message);
        Assert.Equal("TestDto", ex.DtoType);
    }

    [Fact(DisplayName = "Explicit Direct Constructor Tests to guarantee 100% coverage in all coverage runners")]
    public void ExplicitConstructorTests()
    {
        // Local helpers to reduce boilerplate and variable noise
        void AssertBase(Exception ex) => Assert.NotNull(ex);
        void AssertMsg(Exception ex) => Assert.Equal(_testMessage, ex.Message);
        void AssertInner(Exception ex) => Assert.Equal(_testInnerException, ex.InnerException);

        // Parameterless
        AssertBase(new LunoAuthenticationException());
        AssertBase(new LunoForbiddenException());
        AssertBase(new LunoUnauthorizedException());
        AssertBase(new LunoMappingException());
        AssertBase(new LunoRateLimitException());
        AssertBase(new LunoTimeoutException());
        AssertBase(new LunoInsufficientFundsException());
        AssertBase(new LunoOrderRejectedException());
        AssertBase(new LunoResourceNotFoundException());
        AssertBase(new LunoIdempotencyException());
        AssertBase(new LunoValidationException());
        AssertBase(new LunoClientException());
        AssertBase(new TestLunoException());
        AssertBase(new TestLunoSecurityException());
        AssertBase(new TestLunoDataException());
        AssertBase(new TestLunoApiException());
        AssertBase(new TestLunoBusinessRuleException());
        AssertBase(new TestLunoAccountPolicyException());
        AssertBase(new TestLunoMarketStateException());

        // Message
        AssertMsg(new LunoAuthenticationException(_testMessage));
        AssertMsg(new LunoForbiddenException(_testMessage));
        AssertMsg(new LunoUnauthorizedException(_testMessage));
        AssertMsg(new LunoMappingException(_testMessage));
        AssertMsg(new LunoRateLimitException(_testMessage));
        AssertMsg(new LunoTimeoutException(_testMessage));
        AssertMsg(new LunoInsufficientFundsException(_testMessage));
        AssertMsg(new LunoOrderRejectedException(_testMessage));
        AssertMsg(new LunoResourceNotFoundException(_testMessage));
        AssertMsg(new LunoIdempotencyException(_testMessage));
        AssertMsg(new LunoValidationException(_testMessage));
        AssertMsg(new LunoClientException(_testMessage));
        AssertMsg(new TestLunoException(_testMessage));
        AssertMsg(new TestLunoSecurityException(_testMessage));
        AssertMsg(new TestLunoDataException(_testMessage));
        AssertMsg(new TestLunoApiException(_testMessage));
        AssertMsg(new TestLunoBusinessRuleException(_testMessage));
        AssertMsg(new TestLunoAccountPolicyException(_testMessage));
        AssertMsg(new TestLunoMarketStateException(_testMessage));

        // Inner Exception
        AssertInner(new LunoAuthenticationException(_testMessage, _testInnerException));
        AssertInner(new LunoForbiddenException(_testMessage, _testInnerException));
        AssertInner(new LunoUnauthorizedException(_testMessage, _testInnerException));
        AssertInner(new LunoMappingException(_testMessage, _testInnerException));
        AssertInner(new LunoRateLimitException(_testMessage, _testInnerException));
        AssertInner(new LunoTimeoutException(_testMessage, _testInnerException));
        AssertInner(new LunoInsufficientFundsException(_testMessage, _testInnerException));
        AssertInner(new LunoOrderRejectedException(_testMessage, _testInnerException));
        AssertInner(new LunoResourceNotFoundException(_testMessage, _testInnerException));
        AssertInner(new LunoIdempotencyException(_testMessage, _testInnerException));
        AssertInner(new LunoValidationException(_testMessage, _testInnerException));
        AssertInner(new LunoClientException(_testMessage, _testInnerException));
        AssertInner(new TestLunoException(_testMessage, _testInnerException));
        AssertInner(new TestLunoSecurityException(_testMessage, _testInnerException));
        AssertInner(new TestLunoDataException(_testMessage, _testInnerException));
        AssertInner(new TestLunoApiException(_testMessage, _testInnerException));
        AssertInner(new TestLunoBusinessRuleException(_testMessage, _testInnerException));
        AssertInner(new TestLunoAccountPolicyException(_testMessage, _testInnerException));
        AssertInner(new TestLunoMarketStateException(_testMessage, _testInnerException));

        // Properties on LunoApiException
        var exApiFull = new TestLunoApiException(_testMessage, "ErrTest", 400, _testInnerException);
        Assert.Equal("ErrTest", exApiFull.ErrorCode);
        Assert.Equal(400, exApiFull.StatusCode);

        // Constructor mapping for derived exception properties
        var exSecFull = new TestLunoSecurityException(_testMessage, "ErrSec", 401, _testInnerException);
        Assert.Equal("ErrSec", exSecFull.ErrorCode);
        Assert.Equal(401, exSecFull.StatusCode);

        var exBusinessFull = new TestLunoBusinessRuleException(_testMessage, "ErrBus", 400, _testInnerException);
        Assert.Equal("ErrBus", exBusinessFull.ErrorCode);
        Assert.Equal(400, exBusinessFull.StatusCode);

        var exAccountFull = new TestLunoAccountPolicyException(_testMessage, "ErrAcc", 400, _testInnerException);
        Assert.Equal("ErrAcc", exAccountFull.ErrorCode);
        Assert.Equal(400, exAccountFull.StatusCode);

        var exMarketFull = new TestLunoMarketStateException(_testMessage, "ErrMkt", 400, _testInnerException);
        Assert.Equal("ErrMkt", exMarketFull.ErrorCode);
        Assert.Equal(400, exMarketFull.StatusCode);

        var rateLimitFull = new LunoRateLimitException(_testMessage, "ErrTooMany", 429, 60, _testInnerException);
        Assert.Equal("ErrTooMany", rateLimitFull.ErrorCode);
        Assert.Equal(429, rateLimitFull.StatusCode);
        Assert.Equal(60, rateLimitFull.RetryAfter);

        var authFull = new LunoAuthenticationException(_testMessage, "ErrAuth", 401, _testInnerException);
        var forbiddenFull = new LunoForbiddenException(_testMessage, "ErrForb", 403, _testInnerException);
        var unauthorizedFull = new LunoUnauthorizedException(_testMessage, "ErrUnauth", 401, _testInnerException);
        var idempotencyFull = new LunoIdempotencyException(_testMessage, "ErrDup", 409, _testInnerException);
        var orderRejFull = new LunoOrderRejectedException(_testMessage, "ErrOrd", 400, _testInnerException);
        var insufficientFull = new LunoInsufficientFundsException(_testMessage, "ErrFunds", 400, _testInnerException);
        var notFoundFull = new LunoResourceNotFoundException(_testMessage, "ErrNotFound", 404, _testInnerException);
        var validationFull = new LunoValidationException(_testMessage, "ErrVal", 400, _testInnerException);
        var clientFull = new LunoClientException(_testMessage, "ErrClient", 400, _testInnerException);
        var mappingFull = new LunoMappingException(_testMessage, "TestDto", _testInnerException);
        var mappingMeta = new LunoMappingException(_testMessage, "ErrMap", 400, _testInnerException);
        var dataMeta = new LunoDataException(_testMessage, "ErrData", 400, _testInnerException);
        var apiInner = new LunoApiException(_testMessage, _testInnerException);
        var businessMeta = new LunoBusinessRuleException(_testMessage, "ErrBus", 400, _testInnerException);
        var policyMeta = new LunoAccountPolicyException(_testMessage, "ErrPol", 400, _testInnerException);
        var stateMeta = new LunoMarketStateException(_testMessage, "ErrMkt", 400, _testInnerException);
        var securityMeta = new LunoSecurityException(_testMessage, "ErrSec", 401, _testInnerException);

        Assert.Equal("ErrAuth", authFull.ErrorCode);
        Assert.Equal("ErrForb", forbiddenFull.ErrorCode);
        Assert.Equal("ErrUnauth", unauthorizedFull.ErrorCode);
        Assert.Equal("ErrDup", idempotencyFull.ErrorCode);
        Assert.Equal("ErrOrd", orderRejFull.ErrorCode);
        Assert.Equal("ErrFunds", insufficientFull.ErrorCode);
        Assert.Equal("ErrNotFound", notFoundFull.ErrorCode);
        Assert.Equal("ErrVal", validationFull.ErrorCode);
        Assert.Equal("ErrClient", clientFull.ErrorCode);
        Assert.Equal("TestDto", mappingFull.DtoType);
        Assert.Equal("ErrMap", mappingMeta.ErrorCode);
        Assert.Equal("ErrData", dataMeta.ErrorCode);
        Assert.Equal(_testInnerException, apiInner.InnerException);
        Assert.Equal("ErrBus", businessMeta.ErrorCode);
        Assert.Equal("ErrPol", policyMeta.ErrorCode);
        Assert.Equal("ErrMkt", stateMeta.ErrorCode);
        Assert.Equal("ErrSec", securityMeta.ErrorCode);
    }
}
