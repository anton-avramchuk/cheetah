using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidates/{id:guid}", ApiMethod.Delete)]
public record DeleteCandidateRequest([FromRoute] Guid Id) : ICrmRequest;