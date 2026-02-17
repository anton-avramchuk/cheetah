using Crm.VacancyTasks.Domain;
using Shouldly;

namespace Crm.VacancyTasks.Domain.Tests;

public class VacancyTaskTests
{
    private static readonly Guid TestVacancyId = Guid.NewGuid();
    private static readonly Guid TestStateId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidTitle_ShouldCreateEntity()
    {
        // Arrange
        var title = "Test Task";

        // Act
        var entity = VacancyTask.Create(title, TestVacancyId, TestStateId, 1);

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Title.ShouldBe(title);
        entity.VacancyId.ShouldBe(TestVacancyId);
        entity.StateId.ShouldBe(TestStateId);
        entity.Number.ShouldBe(1);
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_WithTitleAndDescription_ShouldCreateEntity()
    {
        // Arrange
        var title = "Test Task";
        var description = "Test Description";

        // Act
        var entity = VacancyTask.Create(title, TestVacancyId, TestStateId, 1, description);

        // Assert
        entity.Title.ShouldBe(title);
        entity.Description.ShouldBe(description);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var entity1 = VacancyTask.Create("Task 1", TestVacancyId, TestStateId, 1);
        var entity2 = VacancyTask.Create("Task 2", TestVacancyId, TestStateId, 2);

        // Assert
        entity1.Id.ShouldNotBe(entity2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string? title)
    {
        // Act
        var act = () => VacancyTask.Create(title!, TestVacancyId, TestStateId, 1);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithValidTitle_ShouldUpdateEntity()
    {
        // Arrange
        var entity = VacancyTask.Create("Original Title", TestVacancyId, TestStateId, 1, "Original Description");

        // Act
        entity.Update("Updated Title", "Updated Description");

        // Assert
        entity.Title.ShouldBe("Updated Title");
        entity.Description.ShouldBe("Updated Description");
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        // Arrange
        var entity = VacancyTask.Create("Original Title", TestVacancyId, TestStateId, 1);
        var originalId = entity.Id;

        // Act
        entity.Update("Updated Title");

        // Assert
        entity.Id.ShouldBe(originalId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidTitle_ShouldThrowArgumentException(string? title)
    {
        // Arrange
        var entity = VacancyTask.Create("Original Title", TestVacancyId, TestStateId, 1);

        // Act
        var act = () => entity.Update(title!);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetState_ShouldUpdateStateId()
    {
        // Arrange
        var entity = VacancyTask.Create("Task", TestVacancyId, TestStateId, 1);
        var newStateId = Guid.NewGuid();

        // Act
        entity.SetState(newStateId);

        // Assert
        entity.StateId.ShouldBe(newStateId);
    }

    [Fact]
    public void SetPriority_ShouldUpdatePriorityId()
    {
        // Arrange
        var entity = VacancyTask.Create("Task", TestVacancyId, TestStateId, 1);
        var priorityId = Guid.NewGuid();

        // Act
        entity.SetPriority(priorityId);

        // Assert
        entity.PriorityId.ShouldBe(priorityId);
    }

    [Fact]
    public void SetPriority_WithNull_ShouldClearPriority()
    {
        // Arrange
        var entity = VacancyTask.Create("Task", TestVacancyId, TestStateId, 1, priorityId: Guid.NewGuid());

        // Act
        entity.SetPriority(null);

        // Assert
        entity.PriorityId.ShouldBeNull();
    }

    [Fact]
    public void Move_ShouldUpdateStateAndOrder()
    {
        // Arrange
        var entity = VacancyTask.Create("Task", TestVacancyId, TestStateId, 1);
        var newStateId = Guid.NewGuid();

        // Act
        entity.Move(newStateId, 5);

        // Assert
        entity.StateId.ShouldBe(newStateId);
        entity.Order.ShouldBe(5);
    }
}
