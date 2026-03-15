using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skill-categories/{id:guid}", ApiMethod.Delete, ServiceName = "SkillCategory")]
public record DeleteSkillCategoryRequest([FromRoute] Guid Id) : ICrmRequest;
