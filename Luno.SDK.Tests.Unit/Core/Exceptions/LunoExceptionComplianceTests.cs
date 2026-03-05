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

    private class TestLunoSecurityException : LunoSecurityException
    {
        public TestLunoSecurityException() : base() { }
        public TestLunoSecurityException(string message) : base(message) { }
        public TestLunoSecurityException(string message, Exception inner) : base(message, inner) { }
    }

    private class TestLunoDataException : LunoDataException
    {
        public TestLunoDataException() : base() { }
        public TestLunoDataException(string message) : base(message) { }
        public TestLunoDataException(string message, Exception inner) : base(message, inner) { }
    }

    public static IEnumerable<object[]> ConcreteExceptionTypes()
    {
        yield return new object[] { typeof(LunoAuthenticationException) };
        yield return new object[] { typeof(LunoForbiddenException) };
        yield return new object[] { typeof(LunoUnauthorizedException) };
        yield return new object[] { typeof(LunoMappingException) };
    }

    [Theory(DisplayName = "Given an exception type, When constructed with parameterless constructor, Then inherit LunoException")]
    [MemberData(nameof(ConcreteExceptionTypes))]
    public void Constructor_GivenParameterless_WhenConstructed_ThenInheritLunoException(Type exceptionType)
    {
        // Act
        var ex = (Exception)Activator.CreateInstance(exceptionType)!;

        // Assert
        Assert.NotNull(ex);
        Assert.IsAssignableFrom<LunoException>(ex);
    }

    [Theory(DisplayName = "Given an exception type, When constructed with message, Then set message and inherit LunoException")]
    [MemberData(nameof(ConcreteExceptionTypes))]
    public void Constructor_GivenMessage_WhenConstructed_ThenSetMessage(Type exceptionType)
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
    public void Constructor_GivenMessageAndInnerException_WhenConstructed_ThenSetBoth(Type exceptionType)
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
    public void LunoMappingException_GivenDtoType_WhenConstructed_ThenSetDtoType()
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

        Assert.NotNull(ex1);
        Assert.NotNull(ex2);
        Assert.NotNull(ex3);
        Assert.NotNull(ex4);
        Assert.NotNull(exBase1);
        Assert.NotNull(exBase2);
        Assert.NotNull(exBase3);

        // Message
        var ex5 = new LunoAuthenticationException(_testMessage);
        var ex6 = new LunoForbiddenException(_testMessage);
        var ex7 = new LunoUnauthorizedException(_testMessage);
        var ex8 = new LunoMappingException(_testMessage);
        var exBase4 = new TestLunoException(_testMessage);
        var exBase5 = new TestLunoSecurityException(_testMessage);
        var exBase6 = new TestLunoDataException(_testMessage);

        Assert.Equal(_testMessage, ex5.Message);
        Assert.Equal(_testMessage, ex6.Message);
        Assert.Equal(_testMessage, ex7.Message);
        Assert.Equal(_testMessage, ex8.Message);
        Assert.Equal(_testMessage, exBase4.Message);
        Assert.Equal(_testMessage, exBase5.Message);
        Assert.Equal(_testMessage, exBase6.Message);

        // Inner Exception
        var ex9 = new LunoAuthenticationException(_testMessage, _testInnerException);
        var ex10 = new LunoForbiddenException(_testMessage, _testInnerException);
        var ex11 = new LunoUnauthorizedException(_testMessage, _testInnerException);
        var ex12 = new LunoMappingException(_testMessage, _testInnerException);
        var exBase7 = new TestLunoException(_testMessage, _testInnerException);
        var exBase8 = new TestLunoSecurityException(_testMessage, _testInnerException);
        var exBase9 = new TestLunoDataException(_testMessage, _testInnerException);

        Assert.Equal(_testInnerException, ex9.InnerException);
        Assert.Equal(_testInnerException, ex10.InnerException);
        Assert.Equal(_testInnerException, ex11.InnerException);
        Assert.Equal(_testInnerException, ex12.InnerException);
        Assert.Equal(_testInnerException, exBase7.InnerException);
        Assert.Equal(_testInnerException, exBase8.InnerException);
        Assert.Equal(_testInnerException, exBase9.InnerException);
    }
}
