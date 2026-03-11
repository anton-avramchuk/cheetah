using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetLocationByIdQuery(Guid Id) : IQuery<LocationModel?>;
