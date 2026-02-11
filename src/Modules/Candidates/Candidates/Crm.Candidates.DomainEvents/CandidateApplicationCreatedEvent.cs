using Cheetah.Core.Events;

namespace Crm.Candidates.DomainEvents;

public record CandidateApplicationCreatedEvent(Guid ApplicationId, Guid CandidateId, Guid VacancyId) : EventBase;
