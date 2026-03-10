using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetStackItemByIdQuery, StackItemModel?>))]
public class GetStackItemByIdQueryHandler(IRepository<StackItem, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetStackItemByIdQuery, StackItemModel?>
{
    public async ValueTask<StackItemModel?> HandleAsync(GetStackItemByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<StackItemModel>(repository.AsNoTrackingQueryable().Where(new EntityByIdSpecification<StackItem>(query.Id)))
            .FirstOrDefaultAsync(ct);
    }
}
