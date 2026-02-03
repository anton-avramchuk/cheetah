using Cheetah.Core.Extensions.Collections;
using Shouldly;

namespace Cheetah.Core.Tests.Extensions.Collections;

public class CollectionExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_WhenCollectionIsNull_ShouldReturnTrue()
    {
        // Arrange
        ICollection<string>? collection = null;

        // Act
        var result = collection.IsNullOrEmpty();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WhenCollectionIsEmpty_ShouldReturnTrue()
    {
        // Arrange
        var collection = new List<string>();

        // Act
        var result = collection.IsNullOrEmpty();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WhenCollectionHasItems_ShouldReturnFalse()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2" };

        // Act
        var result = collection.IsNullOrEmpty();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void AddIfNotContains_WhenItemDoesNotExist_ShouldAddAndReturnTrue()
    {
        // Arrange
        var collection = new List<string> { "item1" };

        // Act
        var result = collection.AddIfNotContains("item2");

        // Assert
        result.ShouldBeTrue();
        collection.ShouldContain("item2");
        collection.Count.ShouldBe(2);
    }

    [Fact]
    public void AddIfNotContains_WhenItemExists_ShouldNotAddAndReturnFalse()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2" };

        // Act
        var result = collection.AddIfNotContains("item1");

        // Assert
        result.ShouldBeFalse();
        collection.Count.ShouldBe(2);
    }

    [Fact]
    public void AddIfNotContains_WithMultipleItems_ShouldAddOnlyNonExistingItems()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2" };
        var itemsToAdd = new[] { "item2", "item3", "item4" };

        // Act
        var addedItems = collection.AddIfNotContains(itemsToAdd);

        // Assert
        addedItems.ShouldBe(new[] { "item3", "item4" });
        collection.Count.ShouldBe(4);
        collection.ShouldBe(new[] { "item1", "item2", "item3", "item4" });
    }

    [Fact]
    public void AddIfNotContains_WithMultipleItems_WhenAllExist_ShouldReturnEmpty()
    {
        // Arrange
        var collection = new List<string> { "item1", "item2" };
        var itemsToAdd = new[] { "item1", "item2" };

        // Act
        var addedItems = collection.AddIfNotContains(itemsToAdd);

        // Assert
        addedItems.ShouldBeEmpty();
        collection.Count.ShouldBe(2);
    }

    [Fact]
    public void AddIfNotContains_WithPredicate_WhenItemDoesNotExist_ShouldAddAndReturnTrue()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = collection.AddIfNotContains(x => x > 5, () => 10);

        // Assert
        result.ShouldBeTrue();
        collection.ShouldContain(10);
        collection.Count.ShouldBe(4);
    }

    [Fact]
    public void AddIfNotContains_WithPredicate_WhenItemExists_ShouldNotAddAndReturnFalse()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = collection.AddIfNotContains(x => x > 2, () => 10);

        // Assert
        result.ShouldBeFalse();
        collection.Count.ShouldBe(3);
        collection.ShouldNotContain(10);
    }

    [Fact]
    public void RemoveAll_WithPredicate_ShouldRemoveMatchingItems()
    {
        // Arrange
        ICollection<int> collection = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var removedItems = collection.RemoveAll(x => x > 3);

        // Assert
        removedItems.Count.ShouldBe(2);
        removedItems.ShouldBe(new[] { 4, 5 });
        collection.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void RemoveAll_WithPredicate_WhenNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        ICollection<int> collection = new List<int> { 1, 2, 3 };

        // Act
        var removedItems = collection.RemoveAll(x => x > 10);

        // Assert
        removedItems.ShouldBeEmpty();
        collection.Count.ShouldBe(3);
    }

    [Fact]
    public void RemoveAll_WithItems_ShouldRemoveSpecifiedItems()
    {
        // Arrange
        var collection = new List<string> { "a", "b", "c", "d" };
        var itemsToRemove = new[] { "b", "d" };

        // Act
        collection.RemoveAll(itemsToRemove);

        // Assert
        collection.ShouldBe(new[] { "a", "c" });
    }

    [Fact]
    public void RemoveAll_WithItems_WhenSomeItemsNotInCollection_ShouldOnlyRemoveExisting()
    {
        // Arrange
        var collection = new List<string> { "a", "b", "c" };
        var itemsToRemove = new[] { "b", "d", "e" };

        // Act
        collection.RemoveAll(itemsToRemove);

        // Assert
        collection.ShouldBe(new[] { "a", "c" });
    }

    [Fact]
    public void RemoveAll_WithEmptyItemsList_ShouldNotChangeCollection()
    {
        // Arrange
        var collection = new List<string> { "a", "b", "c" };
        var itemsToRemove = Array.Empty<string>();

        // Act
        collection.RemoveAll(itemsToRemove);

        // Assert
        collection.ShouldBe(new[] { "a", "b", "c" });
    }
}
