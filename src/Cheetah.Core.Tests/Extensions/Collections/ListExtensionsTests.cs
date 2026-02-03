using Cheetah.Core.Extensions.Collections;
using Shouldly;

namespace Cheetah.Core.Tests.Extensions.Collections;

public class ListExtensionsTests
{
    [Fact]
    public void InsertRange_ShouldInsertItemsAtIndex()
    {
        // Arrange
        var list = new List<int> { 1, 2, 5, 6 };
        var itemsToInsert = new[] { 3, 4 };

        // Act
        list.InsertRange(2, itemsToInsert);

        // Assert
        list.ShouldBe(new[] {1, 2, 3, 4, 5, 6});
    }

    [Fact]
    public void InsertRange_AtBeginning_ShouldInsertItemsAtStart()
    {
        // Arrange
        var list = new List<string> { "c", "d" };
        var itemsToInsert = new[] { "a", "b" };

        // Act
        list.InsertRange(0, itemsToInsert);

        // Assert
        list.ShouldBe(new[] {"a", "b", "c", "d"});
    }

    [Fact]
    public void FindIndex_WhenItemExists_ShouldReturnIndex()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var index = list.FindIndex(x => x == 3);

        // Assert
        index.ShouldBe(2);
    }

    [Fact]
    public void FindIndex_WhenItemDoesNotExist_ShouldReturnMinusOne()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        var index = list.FindIndex(x => x > 10);

        // Assert
        index.ShouldBe(-1);
    }

    [Fact]
    public void AddFirst_ShouldInsertAtBeginning()
    {
        // Arrange
        var list = new List<string> { "b", "c" };

        // Act
        list.AddFirst("a");

        // Assert
        list.ShouldBe(new[] {"a", "b", "c"});
    }

    [Fact]
    public void AddLast_ShouldInsertAtEnd()
    {
        // Arrange
        var list = new List<string> { "a", "b" };

        // Act
        list.AddLast("c");

        // Assert
        list.ShouldBe(new[] {"a", "b", "c"});
    }

    [Fact]
    public void InsertAfter_WithExistingItem_ShouldInsertAfterIt()
    {
        // Arrange
        var list = new List<int> { 1, 2, 4 };

        // Act
        list.InsertAfter(2, 3);

        // Assert
        list.ShouldBe(new[] {1, 2, 3, 4});
    }

    [Fact]
    public void InsertAfter_WithNonExistingItem_ShouldInsertAtBeginning()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        list.InsertAfter(99, 0);

        // Assert
        list.ShouldBe(new[] {0, 1, 2, 3});
    }

    [Fact]
    public void InsertAfter_WithPredicate_WhenItemExists_ShouldInsertAfterIt()
    {
        // Arrange
        var list = new List<int> { 10, 20, 40 };

        // Act
        list.InsertAfter(x => x == 20, 30);

        // Assert
        list.ShouldBe(new[] {10, 20, 30, 40});
    }

    [Fact]
    public void InsertAfter_WithPredicate_WhenNoMatch_ShouldInsertAtBeginning()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        list.InsertAfter(x => x > 100, 0);

        // Assert
        list.ShouldBe(new[] {0, 1, 2, 3});
    }

    [Fact]
    public void InsertBefore_WithExistingItem_ShouldInsertBeforeIt()
    {
        // Arrange
        var list = new List<int> { 1, 3, 4 };

        // Act
        list.InsertBefore(3, 2);

        // Assert
        list.ShouldBe(new[] {1, 2, 3, 4});
    }

    [Fact]
    public void InsertBefore_WithNonExistingItem_ShouldInsertAtEnd()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        list.InsertBefore(99, 4);

        // Assert
        list.ShouldBe(new[] {1, 2, 3, 4});
    }

    [Fact]
    public void InsertBefore_WithPredicate_WhenItemExists_ShouldInsertBeforeIt()
    {
        // Arrange
        var list = new List<int> { 10, 30, 40 };

        // Act
        list.InsertBefore(x => x == 30, 20);

        // Assert
        list.ShouldBe(new[] {10, 20, 30, 40});
    }

    [Fact]
    public void InsertBefore_WithPredicate_WhenNoMatch_ShouldInsertAtEnd()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        list.InsertBefore(x => x > 100, 4);

        // Assert
        list.ShouldBe(new[] {1, 2, 3, 4});
    }

    [Fact]
    public void ReplaceWhile_ShouldReplaceAllMatchingItems()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        list.ReplaceWhile(x => x > 3, 99);

        // Assert
        list.ShouldBe(new[] {1, 2, 3, 99, 99});
    }

    [Fact]
    public void ReplaceWhile_WithFactory_ShouldReplaceWithFactoryResult()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4 };

        // Act
        list.ReplaceWhile(x => x % 2 == 0, x => x * 10);

        // Assert
        list.ShouldBe(new[] {1, 20, 3, 40});
    }

    [Fact]
    public void ReplaceOne_ShouldReplaceFirstMatch()
    {
        // Arrange
        var list = new List<int> { 1, 5, 3, 5 };

        // Act
        list.ReplaceOne(x => x == 5, 2);

        // Assert
        list.ShouldBe(new[] {1, 2, 3, 5});
    }

    [Fact]
    public void ReplaceOne_WhenNoMatch_ShouldNotModifyList()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        list.ReplaceOne(x => x > 10, 99);

        // Assert
        list.ShouldBe(new[] {1, 2, 3});
    }

    [Fact]
    public void ReplaceOne_WithFactory_ShouldReplaceFirstMatchWithFactoryResult()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 2 };

        // Act
        list.ReplaceOne(x => x == 2, x => x * 100);

        // Assert
        list.ShouldBe(new[] {1, 200, 3, 2});
    }

    [Fact]
    public void ReplaceOne_WithItem_ShouldReplaceFirstOccurrence()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c", "b" };

        // Act
        list.ReplaceOne("b", "x");

        // Assert
        list.ShouldBe(new[] {"a", "x", "c", "b"});
    }

    [Fact]
    public void MoveItem_ShouldMoveItemToTargetIndex()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c", "d" };

        // Act
        list.MoveItem(x => x == "d", 1);

        // Assert
        list.ShouldBe(new[] {"a", "d", "b", "c"});
    }

    [Fact]
    public void MoveItem_WhenAlreadyAtTarget_ShouldNotChangeList()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        list.MoveItem(x => x == 2, 1);

        // Assert
        list.ShouldBe(new[] {1, 2, 3});
    }

    [Fact]
    public void MoveItem_WithInvalidTargetIndex_ShouldThrowException()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act & Assert
        var act = () => list.MoveItem(x => x == 2, 10);
        Should.Throw<IndexOutOfRangeException>(act).Message.ShouldContain("targetIndex should be between 0 and");
    }

    [Fact]
    public void GetOrAdd_WhenItemExists_ShouldReturnExistingItem()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };

        // Act
        var result = list.GetOrAdd(x => x == "b", () => "x");

        // Assert
        result.ShouldBe("b");
        list.Count.ShouldBe(3);
    }

    [Fact]
    public void GetOrAdd_WhenItemDoesNotExist_ShouldAddAndReturnNewItem()
    {
        // Arrange
        var list = new List<string> { "a", "b" };

        // Act
        var result = list.GetOrAdd(x => x == "c", () => "c");

        // Assert
        result.ShouldBe("c");
        list.ShouldContain("c");
        list.Count.ShouldBe(3);
    }

    [Fact]
    public void SortByDependencies_ShouldSortCorrectly()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Dependencies = new[] { "B", "C" } },
            new TestItem { Name = "B", Dependencies = new[] { "C" } },
            new TestItem { Name = "C", Dependencies = Array.Empty<string>() }
        };

        // Act
        var sorted = items.SortByDependencies(
            item => items.Where(i => item.Dependencies.Contains(i.Name))
        );

        // Assert
        sorted.Select(x => x.Name).ShouldBe(new[] {"C", "B", "A"});
    }

    [Fact]
    public void SortByDependencies_WithNoDependencies_ShouldReturnOriginalOrder()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Dependencies = Array.Empty<string>() },
            new TestItem { Name = "B", Dependencies = Array.Empty<string>() }
        };

        // Act
        var sorted = items.SortByDependencies(
            item => items.Where(i => item.Dependencies.Contains(i.Name))
        );

        // Assert
        sorted.Select(x => x.Name).ShouldBe(new[] {"A", "B"});
    }

    [Fact]
    public void SortByDependencies_WithCyclicDependency_ShouldThrowException()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new TestItem { Name = "A", Dependencies = new[] { "B" } },
            new TestItem { Name = "B", Dependencies = new[] { "A" } }
        };

        // Act & Assert
        var act = () => items.SortByDependencies(
            item => items.Where(i => item.Dependencies.Contains(i.Name))
        );

        Should.Throw<ArgumentException>(act).Message.ShouldContain("Cyclic dependency found! Item:");
    }

    // Helper class for testing
    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public override string ToString() => Name;
    }
}
