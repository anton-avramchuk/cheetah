using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skill-categories/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(SkillCategoryViewModel), ServiceName = "SkillCategory")]
public record GetSkillCategoryByIdRequest([FromRoute] Guid Id) : ICrmRequest;
