using Crm.Candidates.Domain;
using Shouldly;

namespace Crm.Candidates.Domain.Tests;

public class CandidateApplicationTests
{
    private static readonly Guid TestCandidateId = Guid.NewGuid();
    private static readonly Guid TestVacancyId = Guid.NewGuid();
    private static readonly Guid TestStageId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateEntity()
    {
        var entity = CandidateApplication.Create(TestCandidateId, TestVacancyId, TestStageId);

        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.CandidateId.ShouldBe(TestCandidateId);
        entity.VacancyId.ShouldBe(TestVacancyId);
        entity.StageId.ShouldBe(TestStageId);
        entity.Order.ShouldBe(0);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var entity = CandidateApplication.Create(TestCandidateId, TestVacancyId, TestStageId);

        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void Create_WithOrder_ShouldSetOrder()
    {
        var entity = CandidateApplication.Create(TestCandidateId, TestVacancyId, TestStageId, order: 5);

        entity.Order.ShouldBe(5);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var entity1 = CandidateApplication.Create(TestCandidateId, TestVacancyId, TestStageId);
        var entity2 = CandidateApplication.Create(TestCandidateId, Guid.NewGuid(), TestStageId);

        entity1.Id.ShouldNotBe(entity2.Id);
    }

    [Fact]
    public void Move_ShouldUpdateStageAndOrder()
    {
        var entity = CandidateApplication.Create(TestCandidateId, TestVacancyId, TestStageId);
        var newStageId = Guid.NewGuid();

        entity.Move(newStageId, 3);

        entity.StageId.ShouldBe(newStageId);
        entity.Order.ShouldBe(3);
    }

    [Fact]
    public void Move_ToDifferentStage_ShouldRaiseStageChangedEvent()
    {
        var entity = CandidateApplication.Create(TestCandidateId, TestVacancyId, TestStageId);
        entity.ClearDomainEvents();
        var newStageId = Guid.NewGuid();

        entity.Move(newStageId, 3);

        entity.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void Move_ToSameStage_ShouldNotRaiseStageChangedEvent()
    {
        var entity = CandidateApplication.Create(TestCandidateId, TestVacancyId, TestStageId);
        entity.ClearDomainEvents();

        entity.Move(TestStageId, 5);

        entity.DomainEvents.Count.ShouldBe(0);
        entity.Order.ShouldBe(5);
    }
}
