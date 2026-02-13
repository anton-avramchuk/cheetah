using Cheetah.Admin.Modules.Clients.Api.Client;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Frontend.Models;
using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Admin.Modules.Clients.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<TariffGridViewModel, TariffFormModel, TariffFormModel>))]
public sealed class TariffCrudService : ICrudService<TariffGridViewModel, TariffFormModel, TariffFormModel>
{
    private readonly IAdminClientsService _service;

    public TariffCrudService(IAdminClientsService service)
    {
        _service = service;
    }

    public async Task<GridResult<TariffGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllTariffsAsync(null, ct);
        var mapped = result.Data.Select(TariffGridViewModel.FromResponse).ToList();
        return new GridResult<TariffGridViewModel>(mapped, result.Total);
    }

    public async Task<TariffFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetTariffByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new TariffFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            Currency = entity.Currency,
            IsActive = entity.IsActive
        };
    }

    public async Task<Guid> CreateAsync(TariffFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new CreateTariffRequest(model.Name, description, model.Price, model.Currency, model.IsActive);
        return await _service.CreateTariffAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, TariffFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new UpdateTariffRequest(id, model.Name, description, model.Price, model.Currency, model.IsActive);
        await _service.UpdateTariffAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteTariffAsync(id, ct);
    }
}
