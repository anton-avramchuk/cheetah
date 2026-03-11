using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record DeleteSkillRequest([FromRoute] Guid Id) : ICrmRequest;
