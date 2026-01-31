using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllTariffsQuery, IReadOnlyList<TariffModel>>))]
public class GetAllTariffsQueryHandler : IQueryHandler<GetAllTariffsQuery, IReadOnlyList<TariffModel>>
{
    private readonly ITariffRepository _repository;

    public GetAllTariffsQueryHandler(ITariffRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<IReadOnlyList<TariffModel>> HandleAsync(GetAllTariffsQuery query, CancellationToken ct = default)
    {
        var tariffs = await _repository.GetAllNoTrackingAsync(ct: ct);

        return tariffs
            .Select(t => new TariffModel(
                t.Id,
                t.Name,
                t.Description,
                t.Price,
                t.Currency,
                t.IsActive))
            .ToList();
    }
}
