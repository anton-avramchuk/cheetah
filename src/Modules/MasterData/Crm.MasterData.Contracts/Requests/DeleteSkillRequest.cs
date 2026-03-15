using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skills/{id:guid}", ApiMethod.Delete, ServiceName = "Skill")]
public record DeleteSkillRequest([FromRoute] Guid Id) : ICrmRequest;
