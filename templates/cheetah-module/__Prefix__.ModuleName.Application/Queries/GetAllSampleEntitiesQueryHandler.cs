using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using __Prefix__.ModuleName.Domain.Repositories;

namespace __Prefix__.ModuleName.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllSampleEntitiesQuery, IReadOnlyList<SampleEntityModel>>))]
public class GetAllSampleEntitiesQueryHandler : IQueryHandler<GetAllSampleEntitiesQuery, IReadOnlyList<SampleEntityModel>>
{
    private readonly ISampleEntityRepository _repository;

    public GetAllSampleEntitiesQueryHandler(ISampleEntityRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<SampleEntityModel>> HandleAsync(GetAllSampleEntitiesQuery query, CancellationToken ct = default)
    {
        var entities = await _repository.GetAllNoTrackingAsync(ct: ct);

        return entities
            .Select(e => new SampleEntityModel(e.Id, e.Name, e.Description))
            .ToList();
    }
}
