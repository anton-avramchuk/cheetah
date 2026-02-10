using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCustomersQuery, IReadOnlyList<CustomerModel>>))]
public class GetAllCustomersQueryHandler : IQueryHandler<GetAllCustomersQuery, IReadOnlyList<CustomerModel>>
{
    private readonly IReadOnlyRepository<Customer, Guid> _repository;

    public GetAllCustomersQueryHandler(IRepository<Customer, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<CustomerModel>> HandleAsync(
        GetAllCustomersQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

        return entities
            .Select(e => new CustomerModel(e.Id, e.Name, e.Description, e.DirectionId))
            .ToList();
    }
}
