using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllPositionsQuery : IQuery<IReadOnlyList<PositionModel>>;
