using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCustomersQuery, GridResult<CustomerModel>>))]
public class GetAllCustomersQueryHandler(IGridRepository<Customer> repository)
    : IQueryHandler<GetAllCustomersQuery, GridResult<CustomerModel>>
{
    public async ValueTask<GridResult<CustomerModel>> HandleAsync(GetAllCustomersQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<CustomerModel>(gridRequest, ct);
    }
}
