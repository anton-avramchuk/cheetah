using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record CreateCandidateApplicationCommand(Guid CandidateId, Guid VacancyId, Guid StageId, int Order) : ICommand<Guid>;
