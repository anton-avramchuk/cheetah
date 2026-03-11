using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetSkillByIdQuery(Guid Id) : IQuery<SkillModel?>;
