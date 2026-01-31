using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

public record GetTariffByIdQuery(Guid Id) : IQuery<TariffModel?>;
