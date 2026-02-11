using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-stages/{id:guid}", ApiMethod.Delete, ServiceName = "CandidateStages")]
public record DeleteCandidateStageRequest([FromRoute] Guid Id) : ICrmRequest;
