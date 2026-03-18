using Cheetah.Core.CQRS;

namespace AppName.Application.Queries;

public record GetSampleEntityByIdQuery(Guid Id) : IQuery<SampleEntityModel?>;
