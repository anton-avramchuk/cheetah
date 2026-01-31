using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

public record GetAllTariffsQuery : IQuery<IReadOnlyList<TariffModel>>;
