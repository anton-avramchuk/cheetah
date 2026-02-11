using Cheetah.Core.Events;

namespace Crm.Candidates.DomainEvents;

public record CandidateCreatedEvent(Guid CandidateId, string FirstName, string LastName) : EventBase;
