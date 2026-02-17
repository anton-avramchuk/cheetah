using Crm.Candidates.Domain;
using Shouldly;

namespace Crm.Candidates.Domain.Tests;

public class CandidateStageTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateEntity()
    {
        var entity = CandidateStage.Create("Screening", 1, "#ff0000", true);

        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.Name.ShouldBe("Screening");
        entity.Order.ShouldBe(1);
        entity.Color?.Value.ShouldBe("#ff0000");
        entity.IsDefault.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithMinimalParams_ShouldUseDefaults()
    {
        var entity = CandidateStage.Create("Interview");

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
        var act = () => CandidateStage.Create(name!);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        var entity = CandidateStage.Create("Screening", 1, "#ff0000", true);

        entity.Update("Interview", 2, "#00ff00", false);

        entity.Name.ShouldBe("Interview");
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
        var entity = CandidateStage.Create("Screening");

        var act = () => entity.Update(name!, 0);

        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entity1 = CandidateStage.Create("Stage 1");
        var entity2 = CandidateStage.Create("Stage 2");

        entity1.Id.ShouldNotBe(entity2.Id);
    }
}
