using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Shared;
using Shouldly;

namespace Cheetah.Modules.Deals.Domain.Tests;

public class PipelineTests
{
    private static Pipeline WithStages()
    {
        var p = Pipeline.Create("Sales", isDefault: true);
        p.AddStage("Qualified", 1, 10, StageType.Open);
        p.AddStage("Proposal", 2, 50, StageType.Open);
        p.AddStage("Won", 3, 100, StageType.Won);
        p.AddStage("Lost", 4, 0, StageType.Lost);
        return p;
    }

    [Fact]
    public void Create_IsActiveByDefault()
        => Pipeline.Create("X").IsActive.ShouldBeTrue();

    [Fact]
    public void FirstStage_ReturnsLowestOrderedOpenStage()
        => WithStages().FirstStage().Name.ShouldBe("Qualified");

    [Fact]
    public void FirstStage_NoOpenStage_Throws()
    {
        var p = Pipeline.Create("Empty");
        p.AddStage("Won", 1, 100, StageType.Won);
        Should.Throw<InvalidOperationException>(() => p.FirstStage());
    }

    [Fact]
    public void AddStage_InvalidProbability_Throws()
        => Should.Throw<ArgumentOutOfRangeException>(() => Pipeline.Create("X").AddStage("S", 1, 101, StageType.Open));
}
