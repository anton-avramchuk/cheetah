using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllStackItemsQuery, IReadOnlyList<StackItemModel>>))]
public class GetAllStackItemsQueryHandler : IQueryHandler<GetAllStackItemsQuery, IReadOnlyList<StackItemModel>>
{
    private readonly IReadOnlyRepository<StackItem, Guid> _repository;

    public GetAllStackItemsQueryHandler(IRepository<StackItem, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<StackItemModel>> HandleAsync(
        GetAllStackItemsQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

        return entities
            .Select(e => new StackItemModel(e.Id, e.Name))
            .ToList();
    }
}
