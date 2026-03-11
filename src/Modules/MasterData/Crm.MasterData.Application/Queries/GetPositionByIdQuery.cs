using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetPositionByIdQuery(Guid Id) : IQuery<PositionModel?>;
