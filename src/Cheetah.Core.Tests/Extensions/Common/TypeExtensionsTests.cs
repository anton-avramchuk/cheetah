using Cheetah.Core.Extensions.Common;
using Shouldly;

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
        result.ShouldContain("System.String");
        result.ShouldContain("System.Private.CoreLib");
    }

    [Fact]
    public void IsAssignableTo_Generic_WhenAssignable_ShouldReturnTrue()
    {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var result = type.IsAssignableTo<BaseClass>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAssignableTo_Generic_WhenNotAssignable_ShouldReturnFalse()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var result = type.IsAssignableTo<int>();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsAssignableTo_WhenSameType_ShouldReturnTrue()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var result = type.IsAssignableTo<BaseClass>();

        // Assert
        result.ShouldBeTrue();
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
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAssignableTo_WithInterface_ShouldReturnTrue()
    {
        // Arrange
        var type = typeof(ImplementingClass);

        // Act
        var result = type.IsAssignableTo<ITestInterface>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void GetBaseClasses_ShouldReturnAllBaseClasses()
    {
        // Arrange
        var type = typeof(GrandChildClass);

        // Act
        var result = type.GetBaseClasses(includeObject: false);

        // Assert
        result.ShouldContain(typeof(ChildClass));
        result.ShouldContain(typeof(BaseClass));
        result.ShouldNotContain(typeof(object));
    }

    [Fact]
    public void GetBaseClasses_WithIncludeObject_ShouldIncludeObjectType()
    {
        // Arrange
        var type = typeof(DerivedClass);

        // Act
        var result = type.GetBaseClasses(includeObject: true);

        // Assert
        result.ShouldContain(typeof(BaseClass));
        result.ShouldContain(typeof(object));
    }

    [Fact]
    public void GetBaseClasses_WithNoBaseClass_ShouldReturnEmpty()
    {
        // Arrange
        var type = typeof(BaseClass);

        // Act
        var result = type.GetBaseClasses(includeObject: false);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetBaseClasses_WithStoppingType_ShouldStopAtSpecifiedType()
    {
        // Arrange
        var type = typeof(GrandChildClass);

        // Act
        var result = type.GetBaseClasses(stoppingType: typeof(ChildClass), includeObject: false);

        // Assert
        result.ShouldBeEmpty(); // Should not include ChildClass itself
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
        result[0].ShouldBe(typeof(object));
        result[1].ShouldBe(typeof(BaseClass));
        result[2].ShouldBe(typeof(ChildClass));
    }

    [Fact]
    public void GetBaseClasses_WithValueType_ShouldReturnValueTypeAndObject()
    {
        // Arrange
        var type = typeof(int);

        // Act
        var result = type.GetBaseClasses(includeObject: true);

        // Assert
        result.ShouldContain(typeof(object));
        result.ShouldContain(typeof(ValueType));
    }

    // Test classes and interfaces
    private class BaseClass { }

    private class DerivedClass : BaseClass { }

    private class ChildClass : BaseClass { }

    private class GrandChildClass : ChildClass { }

    private interface ITestInterface { }

    private class ImplementingClass : ITestInterface { }
}
