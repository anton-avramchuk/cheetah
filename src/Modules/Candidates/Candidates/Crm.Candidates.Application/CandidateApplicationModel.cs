namespace Crm.Candidates.Application;

public record CandidateApplicationModel(
    Guid Id,
    Guid CandidateId,
    Guid VacancyId,
    Guid StageId,
    int Order);
