using Crm.VacancyTasks.Domain;
using Shouldly;

namespace Crm.VacancyTasks.Domain.Tests;

public class TaskPriorityTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateEntity()
    {
        var entity = TaskPriority.Create("High", 1, "#ff0000");

        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Name.ShouldBe("High");
        entity.Order.ShouldBe(1);
        entity.Color?.Value.ShouldBe("#ff0000");
    }

    [Fact]
    public void Create_WithMinimalParams_ShouldUseDefaults()
    {
        var entity = TaskPriority.Create("Normal");

        entity.Order.ShouldBe(0);
        entity.Color.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var act = () => TaskPriority.Create(name!);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        var entity = TaskPriority.Create("High", 1, "#ff0000");

        entity.Update("Critical", 0, "#990000");

        entity.Name.ShouldBe("Critical");
        entity.Order.ShouldBe(0);
        entity.Color?.Value.ShouldBe("#990000");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var entity = TaskPriority.Create("High");

        var act = () => entity.Update(name!, 0);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entity1 = TaskPriority.Create("High");
        var entity2 = TaskPriority.Create("Low");

        entity1.Id.ShouldNotBe(entity2.Id);
    }
}
