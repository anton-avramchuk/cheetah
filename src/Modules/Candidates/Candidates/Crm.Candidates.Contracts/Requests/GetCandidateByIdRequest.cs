using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidates/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(CandidateViewModel))]
public record GetCandidateByIdRequest([FromRoute] Guid Id) : ICrmRequest;