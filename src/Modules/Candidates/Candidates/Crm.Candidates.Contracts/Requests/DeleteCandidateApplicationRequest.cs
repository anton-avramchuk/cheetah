using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-applications/{id:guid}", ApiMethod.Delete, ServiceName = "CandidateApplications")]
public record DeleteCandidateApplicationRequest([FromRoute] Guid Id) : ICrmRequest;
