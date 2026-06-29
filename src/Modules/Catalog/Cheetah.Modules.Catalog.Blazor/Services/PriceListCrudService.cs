using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Catalog.Blazor.ViewModels;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Blazor.Services;

/// <summary>
/// CRUD прайс-листов для грида: чтение/удаление — из базового сервиса поверх <see cref="IGridRepository{TEntity}"/>;
/// создание/обновление — через доменные фабрики <see cref="PriceList"/> (строки цен редактируются отдельно).
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICrudService<PriceListGridViewModel, PriceListDetailsViewModel, PriceListCreateViewModel>))]
public sealed class PriceListCrudService(IGridRepository<PriceList> repository)
    : BaseCrudService<PriceList, PriceListGridViewModel, PriceListDetailsViewModel, PriceListCreateViewModel>(repository)
{
    public override async Task<Guid> CreateAsync(PriceListCreateViewModel model, CancellationToken ct = default)
    {
        var priceList = PriceList.Create(model.Name, model.Currency, model.IsDefault, model.ValidFrom, model.ValidTo);
        Repository.Add(priceList);
        await Repository.SaveChangesAsync(ct);
        return priceList.Id;
    }

    public override async Task UpdateAsync(Guid id, PriceListDetailsViewModel model, CancellationToken ct = default)
    {
        var priceList = await Repository.GetByIdAsync(id, ct);
        if (priceList is null) return;

        priceList.Rename(model.Name);
        priceList.ChangeValidity(model.ValidFrom, model.ValidTo);
        priceList.SetDefault(model.IsDefault);
        await Repository.SaveChangesAsync(ct);
    }
}
