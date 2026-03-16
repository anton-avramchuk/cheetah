using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using __Prefix__.ModuleName.Domain;

namespace __Prefix__.ModuleName.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetSampleEntityByIdQuery, SampleEntityModel?>))]
public class GetSampleEntityByIdQueryHandler(IRepository<SampleEntity, Guid> repository, IObjectMapper mapper)
    : IQueryHandler<GetSampleEntityByIdQuery, SampleEntityModel?>
{
    public async ValueTask<SampleEntityModel?> HandleAsync(GetSampleEntityByIdQuery query, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(query.Id, ct);
        return entity is null ? null : mapper.Map<SampleEntityModel>(entity);
    }
}
