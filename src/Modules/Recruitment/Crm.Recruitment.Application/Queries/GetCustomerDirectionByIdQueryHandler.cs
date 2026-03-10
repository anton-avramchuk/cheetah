using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCustomerDirectionByIdQuery, CustomerDirectionModel?>))]
public class GetCustomerDirectionByIdQueryHandler(IRepository<CustomerDirection, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetCustomerDirectionByIdQuery, CustomerDirectionModel?>
{
    public async ValueTask<CustomerDirectionModel?> HandleAsync(GetCustomerDirectionByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<CustomerDirectionModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<CustomerDirection>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
