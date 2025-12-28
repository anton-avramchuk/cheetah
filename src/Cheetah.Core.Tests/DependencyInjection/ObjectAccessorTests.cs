using Cheetah.Core.DependencyInjection;
using FluentAssertions;

namespace Cheetah.Core.Tests.DependencyInjection;

public class ObjectAccessorTests
{
    [Fact]
    public void Constructor_WithoutParameter_ShouldCreateInstanceWithNullValue()
    {
        // Arrange & Act
        var accessor = new ObjectAccessor<string>();

        // Assert
        accessor.Should().NotBeNull();
        accessor.Value.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithParameter_ShouldCreateInstanceWithProvidedValue()
    {
        // Arrange
        const string expectedValue = "test value";

        // Act
        var accessor = new ObjectAccessor<string>(expectedValue);

        // Assert
        accessor.Should().NotBeNull();
        accessor.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Value_WhenSet_ShouldStoreValue()
    {
        // Arrange
        var accessor = new ObjectAccessor<int>();
        const int expectedValue = 42;

        // Act
        accessor.Value = expectedValue;

        // Assert
        accessor.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void Value_WhenSetToNull_ShouldStoreNull()
    {
        // Arrange
        var accessor = new ObjectAccessor<string>("initial value");

        // Act
        accessor.Value = null;

        // Assert
        accessor.Value.Should().BeNull();
    }

    [Fact]
    public void ObjectAccessor_WithReferenceType_ShouldWorkCorrectly()
    {
        // Arrange
        var testObject = new TestClass { Name = "Test", Value = 123 };
        var accessor = new ObjectAccessor<TestClass>(testObject);

        // Act & Assert
        accessor.Value.Should().BeSameAs(testObject);
        accessor.Value!.Name.Should().Be("Test");
        accessor.Value.Value.Should().Be(123);
    }

    [Fact]
    public void ObjectAccessor_WithValueType_ShouldWorkCorrectly()
    {
        // Arrange
        var accessor = new ObjectAccessor<int>(100);

        // Act
        accessor.Value = 200;

        // Assert
        accessor.Value.Should().Be(200);
    }

    [Fact]
    public void ObjectAccessor_ImplementsIObjectAccessor()
    {
        // Arrange
        var accessor = new ObjectAccessor<string>("test");

        // Act & Assert
        accessor.Should().BeAssignableTo<IObjectAccessor<string>>();
    }

    private class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
