using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetTariffByIdQuery, TariffModel?>))]
public class GetTariffByIdQueryHandler : IQueryHandler<GetTariffByIdQuery, TariffModel?>
{
    private readonly ITariffRepository _repository;

    public GetTariffByIdQueryHandler(ITariffRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<TariffModel?> HandleAsync(GetTariffByIdQuery query, CancellationToken ct = default)
    {
        var tariff = await _repository.GetByIdNoTrackingAsync(query.Id, ct);
        if (tariff is null)
            return null;

        return new TariffModel(
            tariff.Id,
            tariff.Name,
            tariff.Description,
            tariff.Price,
            tariff.Currency,
            tariff.IsActive);
    }
}
