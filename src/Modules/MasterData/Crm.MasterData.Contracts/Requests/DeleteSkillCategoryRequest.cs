using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record DeleteSkillCategoryRequest([FromRoute] Guid Id) : ICrmRequest;
