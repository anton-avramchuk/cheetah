using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetSkillCategoryByIdQuery(Guid Id) : IQuery<SkillCategoryModel?>;
