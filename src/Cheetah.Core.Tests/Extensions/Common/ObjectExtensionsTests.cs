using Cheetah.Core.Extensions.Common;
using Shouldly;

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
        result.ShouldBe("Hello");
    }

    [Fact]
    public void As_WithInvalidCast_ShouldThrowException()
    {
        // Arrange
        object obj = "Hello";

        // Act
        var act = () => ObjectExtensions.As<BaseClass>(obj);

        // Assert
        Should.Throw<InvalidCastException>(act);
    }

    [Fact]
    public void As_WithDerivedType_ShouldWork()
    {
        // Arrange
        object obj = new DerivedClass();

        // Act
        var result = ObjectExtensions.As<BaseClass>(obj);

        // Assert
        result.ShouldBeOfType<DerivedClass>();
    }

    [Fact]
    public void To_ShouldConvertIntToDouble()
    {
        // Arrange
        object obj = 42;

        // Act
        var result = obj.To<double>();

        // Assert
        result.ShouldBe(42.0);
    }

    [Fact]
    public void To_ShouldConvertStringToInt()
    {
        // Arrange
        object obj = "123";

        // Act
        var result = obj.To<int>();

        // Assert
        result.ShouldBe(123);
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
        result.ShouldBe(guid);
    }

    [Fact]
    public void To_ShouldConvertStringToDecimal()
    {
        // Arrange
        object obj = "123.45";

        // Act
        var result = obj.To<decimal>();

        // Assert
        result.ShouldBe(123.45m);
    }

    [Fact]
    public void IsIn_WithParams_WhenItemIsInList_ShouldReturnTrue()
    {
        // Arrange
        int value = 2;

        // Act
        var result = value.IsIn(1, 2, 3, 4, 5);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsIn_WithParams_WhenItemIsNotInList_ShouldReturnFalse()
    {
        // Arrange
        int value = 10;

        // Act
        var result = value.IsIn(1, 2, 3, 4, 5);

        // Assert
        result.ShouldBeFalse();
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
        result.ShouldBeTrue();
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
        result.ShouldBeFalse();
    }

    [Fact]
    public void If_WithTrueCondition_ShouldApplyFunction()
    {
        // Arrange
        int value = 5;

        // Act
        var result = value.If(true, x => x * 2);

        // Assert
        result.ShouldBe(10);
    }

    [Fact]
    public void If_WithFalseCondition_ShouldReturnOriginalValue()
    {
        // Arrange
        int value = 5;

        // Act
        var result = value.If(false, x => x * 2);

        // Assert
        result.ShouldBe(5);
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
        executed.ShouldBeTrue();
        result.ShouldBe(5); // Original value returned
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
        executed.ShouldBeFalse();
        result.ShouldBe(5);
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
        result.ShouldBe(12); // 10 + 5 - 3
    }

    // Helper classes
    private class BaseClass { }
    private class DerivedClass : BaseClass { }
}
