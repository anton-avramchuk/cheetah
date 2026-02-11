using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Candidates.Contracts.Requests;

public record DeleteCandidateRequest([FromRoute] Guid Id) : ICrmRequest;