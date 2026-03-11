using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetAllPositionsQuery(
    int Page,
    int PageSize,
    List<SortDescriptor> Sort,
    FilterDescriptor? Filter
) : IQuery<GridResult<PositionModel>>;
