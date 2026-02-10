using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetPositionByIdQuery, PositionModel?>))]
public class GetPositionByIdQueryHandler : IQueryHandler<GetPositionByIdQuery, PositionModel?>
{
    private readonly IReadOnlyRepository<Position, Guid> _repository;

    public GetPositionByIdQueryHandler(IRepository<Position, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<PositionModel?> HandleAsync(GetPositionByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new PositionModel(entity.Id, entity.Name);
    }
}
