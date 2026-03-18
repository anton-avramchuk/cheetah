using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using CustomerEntity = global::Crm.Customer.Domain.Customer;

namespace Crm.Customer.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSampleEntitiesGridQuery, GridResult<CustomerModel>>))]
public class GetSampleEntitiesGridQueryHandler(IGridRepository<CustomerEntity> repository)
    : IQueryHandler<GetSampleEntitiesGridQuery, GridResult<CustomerModel>>
{
    public async ValueTask<GridResult<CustomerModel>> HandleAsync(GetSampleEntitiesGridQuery gridQuery,
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
