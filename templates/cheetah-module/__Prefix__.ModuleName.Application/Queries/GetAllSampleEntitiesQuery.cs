using Cheetah.Core.CQRS;

namespace __Prefix__.ModuleName.Application.Queries;

public record GetAllSampleEntitiesQuery : IQuery<IReadOnlyList<SampleEntityModel>>;
