using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/skills/{id:guid}", ApiMethod.Update, ServiceName = "Skill")]
public record UpdateSkillRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    Guid? SkillCategoryId) : ICrmRequest;
