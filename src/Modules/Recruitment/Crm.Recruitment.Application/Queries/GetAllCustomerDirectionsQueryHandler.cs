using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllCustomerDirectionsQuery, IReadOnlyList<CustomerDirectionModel>>))]
public class GetAllCustomerDirectionsQueryHandler : IQueryHandler<GetAllCustomerDirectionsQuery, IReadOnlyList<CustomerDirectionModel>>
{
    private readonly IReadOnlyRepository<CustomerDirection, Guid> _repository;

    public GetAllCustomerDirectionsQueryHandler(IRepository<CustomerDirection, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<CustomerDirectionModel>> HandleAsync(
        GetAllCustomerDirectionsQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

        return entities
            .Select(e => new CustomerDirectionModel(e.Id, e.Name, e.Description))
            .ToList();
    }
}
