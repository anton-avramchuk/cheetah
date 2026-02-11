using Cheetah.Core.Domain;
using Crm.Candidates.DomainEvents;

namespace Crm.Candidates.Domain;

public class CandidateApplication : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid CandidateId { get; private set; }
    public Candidate Candidate { get; private set; } = null!;
    public Guid VacancyId { get; private set; }
    public Guid StageId { get; private set; }
    public CandidateStage Stage { get; private set; } = null!;
    public int Order { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CandidateApplication()
    {
    }

    public static CandidateApplication Create(Guid candidateId, Guid vacancyId, Guid stageId, int order = 0)
    {
        var entity = new CandidateApplication
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            VacancyId = vacancyId,
            StageId = stageId,
            Order = order
        };

        entity.AddDomainEvent(new CandidateApplicationCreatedEvent(entity.Id, candidateId, vacancyId));
        return entity;
    }

    public void Move(Guid stageId, int order)
    {
        var oldStageId = StageId;
        StageId = stageId;
        Order = order;

        if (oldStageId != stageId)
            AddDomainEvent(new CandidateApplicationStageChangedEvent(Id, oldStageId, stageId));
    }
}
