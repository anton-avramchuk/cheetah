using Cheetah.Core.CQRS;

namespace __Prefix__.ModuleName.Application.Queries;

public record GetSampleEntityByIdQuery(Guid Id) : IQuery<SampleEntityModel?>;
