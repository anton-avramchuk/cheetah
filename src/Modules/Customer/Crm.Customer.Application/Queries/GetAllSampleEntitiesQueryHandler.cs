using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using CustomerEntity = global::Crm.Customer.Domain.Customer;

namespace Crm.Customer.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllSampleEntitiesQuery, GridResult<CustomerModel>>))]
public class GetAllSampleEntitiesQueryHandler(IGridRepository<CustomerEntity> repository)
    : IQueryHandler<GetAllSampleEntitiesQuery, GridResult<CustomerModel>>
{
    public async ValueTask<GridResult<CustomerModel>> HandleAsync(GetAllSampleEntitiesQuery gridQuery,
        CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };

        return await repository.GetGridAsync<CustomerModel>(gridRequest, ct);
    }
}
