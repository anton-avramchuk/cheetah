using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skill-categories", ApiMethod.Create, ServiceName = "SkillCategory")]
public record CreateSkillCategoryRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
