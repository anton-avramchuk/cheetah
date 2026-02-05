using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

public record GetAllTariffsQuery(
    int Page,
    int PageSize,
    List<SortDescriptor> Sort,
    FilterDescriptor? Filter
) : IQuery<GridResult<TariffModel>>;
