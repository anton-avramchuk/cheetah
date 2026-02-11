using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-sources/{id:guid}", ApiMethod.Delete, ServiceName = "CandidateSources")]
public record DeleteCandidateSourceRequest([FromRoute] Guid Id) : ICrmRequest;
