using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

public record GetCandidateByIdRequest([FromRoute] Guid Id) : ICrmRequest;