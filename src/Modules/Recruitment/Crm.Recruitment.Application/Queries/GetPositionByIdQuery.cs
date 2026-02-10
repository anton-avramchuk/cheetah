using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetPositionByIdQuery(Guid Id) : IQuery<PositionModel?>;
