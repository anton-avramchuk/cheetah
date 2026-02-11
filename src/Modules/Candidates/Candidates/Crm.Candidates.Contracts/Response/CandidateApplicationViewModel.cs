using Cheetah.Contracts.Responses;

namespace Crm.Candidates.Contracts.Response;

public record CandidateApplicationViewModel(
    Guid Id,
    Guid CandidateId,
    Guid VacancyId,
    Guid StageId,
    int Order) : ICrmResponse;
