using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCustomerDirectionByIdQuery, CustomerDirectionModel?>))]
public class GetCustomerDirectionByIdQueryHandler : IQueryHandler<GetCustomerDirectionByIdQuery, CustomerDirectionModel?>
{
    private readonly IReadOnlyRepository<CustomerDirection, Guid> _repository;

    public GetCustomerDirectionByIdQueryHandler(IRepository<CustomerDirection, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<CustomerDirectionModel?> HandleAsync(GetCustomerDirectionByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new CustomerDirectionModel(entity.Id, entity.Name, entity.Description);
    }
}
