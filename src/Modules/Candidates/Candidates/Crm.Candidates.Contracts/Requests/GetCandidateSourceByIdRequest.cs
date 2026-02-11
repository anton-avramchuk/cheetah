using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-sources/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(CandidateSourceViewModel), ServiceName = "CandidateSources")]
public record GetCandidateSourceByIdRequest([FromRoute] Guid Id) : ICrmRequest;
