using AppName.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace AppName.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSampleEntityByIdQuery, SampleEntityModel?>))]
public class GetSampleEntityByIdQueryHandler : IQueryHandler<GetSampleEntityByIdQuery, SampleEntityModel?>
{
    private readonly IReadOnlyRepository<SampleEntity, Guid> _repository;

    public GetSampleEntityByIdQueryHandler(IRepository<SampleEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async ValueTask<SampleEntityModel?> HandleAsync(GetSampleEntityByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.AsNoTrackingQueryable()
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct);

        if (entity is null)
            return null;

        return new SampleEntityModel(entity.Id, entity.Name, entity.Description);
    }
}
