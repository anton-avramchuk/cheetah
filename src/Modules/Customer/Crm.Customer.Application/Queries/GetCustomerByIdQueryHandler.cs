using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Customer.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Customer.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCustomerByIdQuery, CustomerModel?>))]
public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerModel?>
{
    private readonly IReadOnlyRepository<Customer, Guid> _repository;

    public GetCustomerByIdQueryHandler(IRepository<Customer, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<CustomerModel?> HandleAsync(GetCustomerByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new CustomerModel(entity.Id, entity.Name, entity.Description);
    }
}