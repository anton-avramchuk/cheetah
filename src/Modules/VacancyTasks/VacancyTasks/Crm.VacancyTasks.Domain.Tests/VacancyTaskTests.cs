using Crm.VacancyTasks.Domain;
using Shouldly;

namespace Crm.VacancyTasks.Domain.Tests;

public class VacancyTaskTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateEntity()
    {
        // Arrange
        var name = "Test Entity";

        // Act
        var entity = VacancyTask.Create(name);

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Name.ShouldBe(name);
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_WithNameAndDescription_ShouldCreateEntity()
    {
        // Arrange
        var name = "Test Entity";
        var description = "Test Description";

        // Act
        var entity = VacancyTask.Create(name, description);

        // Assert
        entity.Name.ShouldBe(name);
        entity.Description.ShouldBe(description);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var entity1 = VacancyTask.Create("Entity 1");
        var entity2 = VacancyTask.Create("Entity 2");

        // Assert
        entity1.Id.ShouldNotBe(entity2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => VacancyTask.Create(name!);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithValidName_ShouldUpdateEntity()
    {
        // Arrange
        var entity = VacancyTask.Create("Original Name", "Original Description");

        // Act
        entity.Update("Updated Name", "Updated Description");

        // Assert
        entity.Name.ShouldBe("Updated Name");
        entity.Description.ShouldBe("Updated Description");
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        // Arrange
        var entity = VacancyTask.Create("Original Name");
        var originalId = entity.Id;

        // Act
        entity.Update("Updated Name");

        // Assert
        entity.Id.ShouldBe(originalId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var entity = VacancyTask.Create("Original Name");

        // Act
        var act = () => entity.Update(name!);

        // Assert
        Should.Throw<ArgumentException>(act);
    }
}