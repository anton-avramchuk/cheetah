using Cheetah.Core.Events;

namespace Crm.Candidates.DomainEvents;

public record CandidateApplicationStageChangedEvent(Guid ApplicationId, Guid OldStageId, Guid NewStageId) : EventBase;
