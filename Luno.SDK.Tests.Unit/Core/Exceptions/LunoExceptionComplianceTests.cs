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
    public class TestLunoException : LunoException
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

    [Fact(DisplayName = "Explicit Direct Constructor Tests to guarantee 100% FATALITY in all coverage runners")]
    public void ExplicitConstructorTests()
    {
        // Parameterless
        var ex1 = new LunoAuthenticationException();
        var ex2 = new LunoForbiddenException();
        var ex3 = new LunoUnauthorizedException();
        var ex4 = new LunoMappingException();
        var exBase1 = new TestLunoException();
        var exBase2 = new TestLunoSecurityException();
        var exBase3 = new TestLunoDataException();
        var exBase4 = new TestLunoApiException();
        var exBase5 = new TestLunoBusinessRuleException();
        var exBase6 = new TestLunoAccountPolicyException();
        var exBase7 = new TestLunoMarketStateException();

        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
        Assert.NotNull(ex3);
        Assert.NotNull(ex4);
        Assert.NotNull(exBase1);
        Assert.NotNull(exBase2);
        Assert.NotNull(exBase3);
        Assert.NotNull(exBase4);
        Assert.NotNull(exBase5);
        Assert.NotNull(exBase6);
        Assert.NotNull(exBase7);

        // Message
        var ex5 = new LunoAuthenticationException(_testMessage);
        var ex6 = new LunoForbiddenException(_testMessage);
        var ex7 = new LunoUnauthorizedException(_testMessage);
        var ex8 = new LunoMappingException(_testMessage);
        var exBase8 = new TestLunoException(_testMessage);
        var exBase9 = new TestLunoSecurityException(_testMessage);
        var exBase10 = new TestLunoDataException(_testMessage);
        var exBase11 = new TestLunoApiException(_testMessage);
        var exBase12 = new TestLunoBusinessRuleException(_testMessage);
        var exBase13 = new TestLunoAccountPolicyException(_testMessage);
        var exBase14 = new TestLunoMarketStateException(_testMessage);

        Assert.Equal(_testMessage, ex5.Message);
        Assert.Equal(_testMessage, ex6.Message);
        Assert.Equal(_testMessage, ex7.Message);
        Assert.Equal(_testMessage, ex8.Message);
        Assert.Equal(_testMessage, exBase8.Message);
        Assert.Equal(_testMessage, exBase9.Message);
        Assert.Equal(_testMessage, exBase10.Message);
        Assert.Equal(_testMessage, exBase11.Message);
        Assert.Equal(_testMessage, exBase12.Message);
        Assert.Equal(_testMessage, exBase13.Message);
        Assert.Equal(_testMessage, exBase14.Message);

        // Inner Exception
        var ex9 = new LunoAuthenticationException(_testMessage, _testInnerException);
        var ex10 = new LunoForbiddenException(_testMessage, _testInnerException);
        var ex11 = new LunoUnauthorizedException(_testMessage, _testInnerException);
        var ex12 = new LunoMappingException(_testMessage, _testInnerException);
        var exBase15 = new TestLunoException(_testMessage, _testInnerException);
        var exBase16 = new TestLunoSecurityException(_testMessage, _testInnerException);
        var exBase17 = new TestLunoDataException(_testMessage, _testInnerException);
        var exBase18 = new TestLunoApiException(_testMessage, _testInnerException);
        var exBase19 = new TestLunoBusinessRuleException(_testMessage, _testInnerException);
        var exBase20 = new TestLunoAccountPolicyException(_testMessage, _testInnerException);
        var exBase21 = new TestLunoMarketStateException(_testMessage, _testInnerException);

        Assert.Equal(_testInnerException, ex9.InnerException);
        Assert.Equal(_testInnerException, ex10.InnerException);
        Assert.Equal(_testInnerException, ex11.InnerException);
        Assert.Equal(_testInnerException, ex12.InnerException);
        Assert.Equal(_testInnerException, exBase15.InnerException);
        Assert.Equal(_testInnerException, exBase16.InnerException);
        Assert.Equal(_testInnerException, exBase17.InnerException);
        Assert.Equal(_testInnerException, exBase18.InnerException);
        Assert.Equal(_testInnerException, exBase19.InnerException);
        Assert.Equal(_testInnerException, exBase20.InnerException);
        Assert.Equal(_testInnerException, exBase21.InnerException);

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

        Assert.Equal("ErrAuth", authFull.ErrorCode);
        Assert.Equal("ErrForb", forbiddenFull.ErrorCode);
        Assert.Equal("ErrUnauth", unauthorizedFull.ErrorCode);
        Assert.Equal("ErrDup", idempotencyFull.ErrorCode);
        Assert.Equal("ErrOrd", orderRejFull.ErrorCode);
        Assert.Equal("ErrFunds", insufficientFull.ErrorCode);
        Assert.Equal("ErrNotFound", notFoundFull.ErrorCode);
        Assert.Equal("ErrVal", validationFull.ErrorCode);
    }
}
