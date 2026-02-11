using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-stages/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(CandidateStageViewModel), ServiceName = "CandidateStages")]
public record GetCandidateStageByIdRequest([FromRoute] Guid Id) : ICrmRequest;
