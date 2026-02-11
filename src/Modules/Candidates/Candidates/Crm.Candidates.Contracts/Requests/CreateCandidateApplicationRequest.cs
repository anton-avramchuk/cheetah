using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-applications", ApiMethod.Create, ServiceName = "CandidateApplications")]
public record CreateCandidateApplicationRequest(
    Guid CandidateId,
    Guid VacancyId,
    Guid StageId,
    int Order) : ICrmRequest;
