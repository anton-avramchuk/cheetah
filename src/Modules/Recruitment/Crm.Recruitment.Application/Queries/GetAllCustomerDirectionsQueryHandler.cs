using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCustomerDirectionsQuery, GridResult<CustomerDirectionModel>>))]
public class GetAllCustomerDirectionsQueryHandler(IGridRepository<CustomerDirection> repository)
    : IQueryHandler<GetAllCustomerDirectionsQuery, GridResult<CustomerDirectionModel>>
{
    public async ValueTask<GridResult<CustomerDirectionModel>> HandleAsync(GetAllCustomerDirectionsQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<CustomerDirectionModel>(gridRequest, ct);
    }
}
