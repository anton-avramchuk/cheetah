using __Prefix__.ModuleName.Domain;
using Shouldly;

namespace __Prefix__.ModuleName.Domain.Tests;

public class SampleEntityTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateEntity()
    {
        var entity = SampleEntity.Create("Test Entity");

        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Name.ShouldBe("Test Entity");
        entity.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_WithNameAndDescription_ShouldCreateEntity()
    {
        var entity = SampleEntity.Create("Test Entity", "Test Description");

        entity.Name.ShouldBe("Test Entity");
        entity.Description.ShouldBe("Test Description");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entity1 = SampleEntity.Create("Entity 1");
        var entity2 = SampleEntity.Create("Entity 2");

        entity1.Id.ShouldNotBe(entity2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        Should.Throw<ArgumentException>(() => SampleEntity.Create(name!));
    }

    [Fact]
    public void Update_WithValidName_ShouldUpdateEntity()
    {
        var entity = SampleEntity.Create("Original Name", "Original Description");

        entity.Update("Updated Name", "Updated Description");

        entity.Name.ShouldBe("Updated Name");
        entity.Description.ShouldBe("Updated Description");
    }

    [Fact]
    public void Update_ShouldNotChangeId()
    {
        var entity = SampleEntity.Create("Original Name");
        var originalId = entity.Id;

        entity.Update("Updated Name");

        entity.Id.ShouldBe(originalId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var entity = SampleEntity.Create("Original Name");

        Should.Throw<ArgumentException>(() => entity.Update(name!));
    }
}
