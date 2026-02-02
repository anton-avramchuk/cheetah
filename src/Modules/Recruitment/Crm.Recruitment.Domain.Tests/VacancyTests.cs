using Crm.Recruitment.Domain;
using FluentAssertions;

namespace Crm.Recruitment.Domain.Tests;

public class VacancyTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateEntity()
    {
        // Arrange
        var name = "Test Entity";

        // Act
        var entity = Vacancy.Create(name);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Name.Should().Be(name);
        entity.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithNameAndDescription_ShouldCreateEntity()
    {
        // Arrange
        var name = "Test Entity";
        var description = "Test Description";

        // Act
        var entity = Vacancy.Create(name, description);

        // Assert
        entity.Name.Should().Be(name);
        entity.Description.Should().Be(description);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act
        var entity1 = Vacancy.Create("Entity 1");
        var entity2 = Vacancy.Create("Entity 2");

        // Assert
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => Vacancy.Create(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidName_ShouldUpdateEntity()
    {
        // Arrange
        var entity = Vacancy.Create("Original Name", "Original Description");

        // Act
        entity.Update("Updated Name", "Updated Description");

        // Assert
        entity.Name.Should().Be("Updated Name");
        entity.Description.Should().Be("Updated Description");
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        // Arrange
        var entity = Vacancy.Create("Original Name");
        var originalId = entity.Id;

        // Act
        entity.Update("Updated Name");

        // Assert
        entity.Id.Should().Be(originalId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var entity = Vacancy.Create("Original Name");

        // Act
        var act = () => entity.Update(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}