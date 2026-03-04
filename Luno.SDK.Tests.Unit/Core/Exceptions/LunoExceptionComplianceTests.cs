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

    // We can't test LunoException, LunoSecurityException, and LunoDataException directly as they are abstract,
    // but they are covered by their concrete implementations calling base().

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
}
