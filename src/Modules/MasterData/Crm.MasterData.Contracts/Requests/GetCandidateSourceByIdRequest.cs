using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/candidate-sources/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(CandidateSourceViewModel), ServiceName = "CandidateSource")]
public record GetCandidateSourceByIdRequest([FromRoute] Guid Id) : ICrmRequest;
