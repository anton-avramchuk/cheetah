using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/candidate-sources/{id:guid}", ApiMethod.Delete, ServiceName = "CandidateSource")]
public record DeleteCandidateSourceRequest([FromRoute] Guid Id) : ICrmRequest;
