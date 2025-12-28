using Cheetah.Core.Extensions.Common;
using FluentAssertions;

namespace Cheetah.Core.Tests.Extensions.Common;

public class TypeExtensionsTests
{
    [Fact]
    public void GetFullNameWithAssemblyName_ShouldReturnFullNameWithAssembly()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var result = type.GetFullNameWithAssemblyName();

        // Assert
        result.Should().Contain("System.String");
        result.Should().Contain("System.Private.CoreLib");
    }

    [Fact]
    public void IsAssignableTo_Generic_WhenAssignable_ShouldReturnTrue()
    {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var result = type.IsAssignableTo<BaseClass>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAssignableTo_Generic_WhenNotAssignable_ShouldReturnFalse()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var result = type.IsAssignableTo<int>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAssignableTo_WhenSameType_ShouldReturnTrue()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var result = type.IsAssignableTo<BaseClass>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAssignableTo_NonGeneric_WhenAssignable_ShouldReturnTrue()
    {
        // Arrange
        var type = typeof(DerivedClass);
        var targetType = typeof(BaseClass);

        // Act
        var result = type.IsAssignableTo(targetType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAssignableTo_WithInterface_ShouldReturnTrue()
    {
        // Arrange
        var type = typeof(ImplementingClass);

        // Act
        var result = type.IsAssignableTo<ITestInterface>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBaseClasses_ShouldReturnAllBaseClasses()
    {
        // Arrange
        var type = typeof(GrandChildClass);

        // Act
        var result = type.GetBaseClasses(includeObject: false);

        // Assert
        result.Should().Contain(typeof(ChildClass));
        result.Should().Contain(typeof(BaseClass));
        result.Should().NotContain(typeof(object));
    }

    [Fact]
    public void GetBaseClasses_WithIncludeObject_ShouldIncludeObjectType()
    {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var result = type.GetBaseClasses(includeObject: true);

        // Assert
        result.Should().Contain(typeof(BaseClass));
        result.Should().Contain(typeof(object));
    }

    [Fact]
    public void GetBaseClasses_WithNoBaseClass_ShouldReturnEmpty()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var result = type.GetBaseClasses(includeObject: false);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetBaseClasses_WithStoppingType_ShouldStopAtSpecifiedType()
    {
        // Arrange
        var type = typeof(GrandChildClass);

        // Act
        var result = type.GetBaseClasses(stoppingType: typeof(ChildClass), includeObject: false);

        // Assert
        result.Should().BeEmpty(); // Should not include ChildClass itself
    }

    [Fact]
    public void GetBaseClasses_ShouldReturnInCorrectOrder()
    {
        // Arrange
        var type = typeof(GrandChildClass);

        // Act
        var result = type.GetBaseClasses(includeObject: true);

        // Assert
        // Should be ordered from base to most derived
        result[0].Should().Be(typeof(object));
        result[1].Should().Be(typeof(BaseClass));
        result[2].Should().Be(typeof(ChildClass));
    }

    [Fact]
    public void GetBaseClasses_WithValueType_ShouldReturnValueTypeAndObject()
    {
        // Arrange
        var type = typeof(int);

        // Act
        var result = type.GetBaseClasses(includeObject: true);

        // Assert
        result.Should().Contain(typeof(object));
        result.Should().Contain(typeof(ValueType));
    }

    // Test classes and interfaces
    private class BaseClass { }

    private class DerivedClass : BaseClass { }

    private class ChildClass : BaseClass { }

    private class GrandChildClass : ChildClass { }

    private interface ITestInterface { }

    private class ImplementingClass : ITestInterface { }
}
