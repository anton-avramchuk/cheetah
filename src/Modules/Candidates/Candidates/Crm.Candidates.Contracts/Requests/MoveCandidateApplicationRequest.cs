using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-applications/{id:guid}/move", ApiMethod.Patch, ServiceName = "CandidateApplications")]
public record MoveCandidateApplicationRequest([FromRoute] Guid Id, Guid StageId, int Order) : ICrmRequest;
