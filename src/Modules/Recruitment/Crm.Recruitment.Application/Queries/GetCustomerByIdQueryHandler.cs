using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCustomerByIdQuery, CustomerModel?>))]
public class GetCustomerByIdQueryHandler(IRepository<Customer, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetCustomerByIdQuery, CustomerModel?>
{
    public async ValueTask<CustomerModel?> HandleAsync(GetCustomerByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<CustomerModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<Customer>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
