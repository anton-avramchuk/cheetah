using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using __Prefix__.ModuleName.Domain.Repositories;

namespace __Prefix__.ModuleName.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSampleEntityByIdQuery, SampleEntityModel?>))]
public class GetSampleEntityByIdQueryHandler : IQueryHandler<GetSampleEntityByIdQuery, SampleEntityModel?>
{
    private readonly ISampleEntityRepository _repository;

    public GetSampleEntityByIdQueryHandler(ISampleEntityRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<SampleEntityModel?> HandleAsync(GetSampleEntityByIdQuery query, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdNoTrackingAsync(query.Id, ct);
        if (entity is null)
            return null;

        return new SampleEntityModel(entity.Id, entity.Name, entity.Description);
    }
}
