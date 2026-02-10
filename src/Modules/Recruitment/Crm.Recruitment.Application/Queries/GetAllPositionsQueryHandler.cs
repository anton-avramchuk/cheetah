using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllPositionsQuery, IReadOnlyList<PositionModel>>))]
public class GetAllPositionsQueryHandler : IQueryHandler<GetAllPositionsQuery, IReadOnlyList<PositionModel>>
{
    private readonly IReadOnlyRepository<Position, Guid> _repository;

    public GetAllPositionsQueryHandler(IRepository<Position, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<PositionModel>> HandleAsync(
        GetAllPositionsQuery query,
        CancellationToken ct = default)
    {
        var entities = await _repository.AsNoTrackingQueryable()
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

        return entities
            .Select(e => new PositionModel(e.Id, e.Name))
            .ToList();
    }
}
