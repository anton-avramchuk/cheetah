using System.Collections.Concurrent;
using Cheetah.Core.Extensions.Collections;
using FluentAssertions;

namespace Cheetah.Core.Tests.Extensions.Collections;

public class DictionaryExtensionsTests
{
    [Fact]
    public void GetOrDefault_Dictionary_WhenKeyExists_ShouldReturnValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "key1", 10 },
            { "key2", 20 }
        };

        // Act
        var result = dictionary.GetOrDefault("key1");

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void GetOrDefault_Dictionary_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "key1", 10 }
        };

        // Act
        var result = dictionary.GetOrDefault("nonexistent");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetOrDefault_IDictionary_WhenKeyExists_ShouldReturnValue()
    {
        // Arrange
        IDictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var result = dictionary.GetOrDefault("key2");

        // Assert
        result.Should().Be("value2");
    }

    [Fact]
    public void GetOrDefault_IDictionary_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        IDictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { "key1", "value1" }
        };

        // Act
        var result = dictionary.GetOrDefault("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetOrDefault_IReadOnlyDictionary_WhenKeyExists_ShouldReturnValue()
    {
        // Arrange
        IReadOnlyDictionary<int, string> dictionary = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" }
        };

        // Act
        var result = dictionary.GetOrDefault(2);

        // Assert
        result.Should().Be("two");
    }

    [Fact]
    public void GetOrDefault_IReadOnlyDictionary_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        IReadOnlyDictionary<int, string> dictionary = new Dictionary<int, string>
        {
            { 1, "one" }
        };

        // Act
        var result = dictionary.GetOrDefault(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetOrDefault_ConcurrentDictionary_WhenKeyExists_ShouldReturnValue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        dictionary.TryAdd("key1", 100);

        // Act
        var result = dictionary.GetOrDefault("key1");

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void GetOrDefault_ConcurrentDictionary_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        var result = dictionary.GetOrDefault("nonexistent");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetOrAdd_IDictionary_WhenKeyDoesNotExist_ShouldAddAndReturnValue()
    {
        // Arrange
        IDictionary<string, int> dictionary = new Dictionary<string, int>();

        // Act
        var result = dictionary.GetOrAdd("key1", k => 42);

        // Assert
        result.Should().Be(42);
        dictionary.Should().ContainKey("key1");
        dictionary["key1"].Should().Be(42);
    }

    [Fact]
    public void GetOrAdd_IDictionary_WhenKeyExists_ShouldReturnExistingValue()
    {
        // Arrange
        IDictionary<string, int> dictionary = new Dictionary<string, int>
        {
            { "key1", 10 }
        };

        // Act
        var result = dictionary.GetOrAdd("key1", k => 99);

        // Assert
        result.Should().Be(10);
        dictionary["key1"].Should().Be(10);
    }

    [Fact]
    public void GetOrAdd_IDictionary_WithFactoryNoParam_ShouldAddAndReturnValue()
    {
        // Arrange
        IDictionary<string, string> dictionary = new Dictionary<string, string>();

        // Act
        var result = dictionary.GetOrAdd("key1", () => "created value");

        // Assert
        result.Should().Be("created value");
        dictionary.Should().ContainKey("key1");
    }

    [Fact]
    public void GetOrAdd_ConcurrentDictionary_ShouldAddAndReturnValue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();

        // Act
        var result = dictionary.GetOrAdd("key1", () => 123);

        // Assert
        result.Should().Be(123);
        dictionary.Should().ContainKey("key1");
    }

    [Fact]
    public void GetOrAdd_ConcurrentDictionary_WhenKeyExists_ShouldReturnExistingValue()
    {
        // Arrange
        var dictionary = new ConcurrentDictionary<string, int>();
        dictionary.TryAdd("key1", 50);

        // Act
        var result = dictionary.GetOrAdd("key1", () => 999);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public void ConvertToDynamicObject_ShouldCreateExpandoObject()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            { "Name", "John" },
            { "Age", 30 },
            { "IsActive", true }
        };

        // Act
        dynamic result = dictionary.ConvertToDynamicObject();

        // Assert
        ((object)result).Should().NotBeNull();
        ((string)result.Name).Should().Be("John");
        ((int)result.Age).Should().Be(30);
        ((bool)result.IsActive).Should().BeTrue();
    }

    [Fact]
    public void ConvertToDynamicObject_WithEmptyDictionary_ShouldCreateEmptyExpandoObject()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>();

        // Act
        dynamic result = dictionary.ConvertToDynamicObject();

        // Assert
        ((object)result).Should().NotBeNull();
    }

    [Fact]
    public void ConvertToDynamicObject_ShouldAllowAddingNewProperties()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            { "Initial", "value" }
        };

        // Act
        dynamic result = dictionary.ConvertToDynamicObject();
        result.NewProperty = "new value";

        // Assert
        ((string)result.Initial).Should().Be("value");
        ((string)result.NewProperty).Should().Be("new value");
    }

    [Fact]
    public void GetOrAdd_FactoryReceivesKey_ShouldPassKeyToFactory()
    {
        // Arrange
        IDictionary<string, string> dictionary = new Dictionary<string, string>();
        string? receivedKey = null;

        // Act
        var result = dictionary.GetOrAdd("myKey", key =>
        {
            receivedKey = key;
            return $"value for {key}";
        });

        // Assert
        receivedKey.Should().Be("myKey");
        result.Should().Be("value for myKey");
    }
}
