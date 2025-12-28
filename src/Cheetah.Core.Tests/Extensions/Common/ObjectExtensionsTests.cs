using Cheetah.Core.Extensions.Common;
using FluentAssertions;

namespace Cheetah.Core.Tests.Extensions.Common;

public class ObjectExtensionsTests
{
    [Fact]
    public void As_ShouldCastToSpecifiedType()
    {
        // Arrange
        object obj = "Hello";

        // Act
        var result = ObjectExtensions.As<string>(obj);

        // Assert
        result.Should().Be("Hello");
    }

    [Fact]
    public void As_WithInvalidCast_ShouldThrowException()
    {
        // Arrange
        object obj = "Hello";

        // Act
        var act = () => ObjectExtensions.As<BaseClass>(obj);

        // Assert
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void As_WithDerivedType_ShouldWork()
    {
        // Arrange
        object obj = new DerivedClass();

        // Act
        var result = ObjectExtensions.As<BaseClass>(obj);

        // Assert
        result.Should().BeOfType<DerivedClass>();
    }

    [Fact]
    public void To_ShouldConvertIntToDouble()
    {
        // Arrange
        object obj = 42;

        // Act
        var result = obj.To<double>();

        // Assert
        result.Should().Be(42.0);
    }

    [Fact]
    public void To_ShouldConvertStringToInt()
    {
        // Arrange
        object obj = "123";

        // Act
        var result = obj.To<int>();

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void To_ShouldConvertStringToGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        object obj = guid.ToString();

        // Act
        var result = obj.To<Guid>();

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void To_ShouldConvertStringToDecimal()
    {
        // Arrange
        object obj = "123.45";

        // Act
        var result = obj.To<decimal>();

        // Assert
        result.Should().Be(123.45m);
    }

    [Fact]
    public void IsIn_WithParams_WhenItemIsInList_ShouldReturnTrue()
    {
        // Arrange
        int value = 2;

        // Act
        var result = value.IsIn(1, 2, 3, 4, 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIn_WithParams_WhenItemIsNotInList_ShouldReturnFalse()
    {
        // Arrange
        int value = 10;

        // Act
        var result = value.IsIn(1, 2, 3, 4, 5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsIn_WithEnumerable_WhenItemIsInList_ShouldReturnTrue()
    {
        // Arrange
        string value = "cat";
        var list = new[] { "dog", "cat", "bird" };

        // Act
        var result = value.IsIn(list);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIn_WithEnumerable_WhenItemIsNotInList_ShouldReturnFalse()
    {
        // Arrange
        string value = "fish";
        var list = new[] { "dog", "cat", "bird" };

        // Act
        var result = value.IsIn(list);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void If_WithTrueCondition_ShouldApplyFunction()
    {
        // Arrange
        int value = 5;

        // Act
        var result = value.If(true, x => x * 2);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void If_WithFalseCondition_ShouldReturnOriginalValue()
    {
        // Arrange
        int value = 5;

        // Act
        var result = value.If(false, x => x * 2);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void If_WithAction_WhenConditionIsTrue_ShouldExecuteAction()
    {
        // Arrange
        var executed = false;
        int value = 5;

        // Act
        var result = value.If(true, x => executed = true);

        // Assert
        executed.Should().BeTrue();
        result.Should().Be(5); // Original value returned
    }

    [Fact]
    public void If_WithAction_WhenConditionIsFalse_ShouldNotExecuteAction()
    {
        // Arrange
        var executed = false;
        int value = 5;

        // Act
        var result = value.If(false, x => executed = true);

        // Assert
        executed.Should().BeFalse();
        result.Should().Be(5);
    }

    [Fact]
    public void If_CanBeChained()
    {
        // Arrange
        int value = 10;

        // Act
        var result = value
            .If(true, x => x + 5)
            .If(false, x => x * 2)
            .If(true, x => x - 3);

        // Assert
        result.Should().Be(12); // 10 + 5 - 3
    }

    // Helper classes
    private class BaseClass { }
    private class DerivedClass : BaseClass { }
}
