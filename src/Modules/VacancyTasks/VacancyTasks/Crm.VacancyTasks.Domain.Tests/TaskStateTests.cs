using Crm.VacancyTasks.Domain;
using Shouldly;

namespace Crm.VacancyTasks.Domain.Tests;

public class TaskStateTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateEntity()
    {
        var entity = TaskState.Create("To Do", 1, "#ff0000", true);

        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Name.ShouldBe("To Do");
        entity.Order.ShouldBe(1);
        entity.Color?.Value.ShouldBe("#ff0000");
        entity.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithMinimalParams_ShouldUseDefaults()
    {
        var entity = TaskState.Create("Done");

        entity.Order.ShouldBe(0);
        entity.Color.ShouldBeNull();
        entity.IsDefault.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var act = () => TaskState.Create(name!);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        var entity = TaskState.Create("To Do", 1, "#ff0000", true);

        entity.Update("In Progress", 2, "#00ff00", false);

        entity.Name.ShouldBe("In Progress");
        entity.Order.ShouldBe(2);
        entity.Color?.Value.ShouldBe("#00ff00");
        entity.IsDefault.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var entity = TaskState.Create("To Do");

        var act = () => entity.Update(name!, 0);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entity1 = TaskState.Create("State 1");
        var entity2 = TaskState.Create("State 2");

        entity1.Id.ShouldNotBe(entity2.Id);
    }
}
