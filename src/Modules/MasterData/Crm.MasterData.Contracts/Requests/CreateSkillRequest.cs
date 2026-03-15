using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skills", ApiMethod.Create, ServiceName = "Skill")]
public record CreateSkillRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    Guid? SkillCategoryId) : ICrmRequest;
