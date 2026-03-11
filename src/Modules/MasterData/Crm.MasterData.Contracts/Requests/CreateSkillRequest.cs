using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record CreateSkillRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    Guid? SkillCategoryId) : ICrmRequest;
