using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Frontend.Models;

namespace Crm.Recruitment.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<VacancyStateGridViewModel, VacancyStateFormModel, VacancyStateFormModel>))]
public sealed class VacancyStateCrudService : ICrudService<VacancyStateGridViewModel, VacancyStateFormModel, VacancyStateFormModel>
{
    private readonly IVacancyStatesService _service;

    public VacancyStateCrudService(IVacancyStatesService service)
    {
        _service = service;
    }

    public async Task<GridResult<VacancyStateGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(VacancyStateGridViewModel.FromResponse).ToList();
        return new GridResult<VacancyStateGridViewModel>(mapped, mapped.Count);
    }

    public async Task<VacancyStateFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new VacancyStateFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Order = entity.Order,
            Color = entity.Color,
            IsDefault = entity.IsDefault
        };
    }

    public async Task<Guid> CreateAsync(VacancyStateFormModel model, CancellationToken ct = default)
    {
        var request = new CreateVacancyStateRequest(model.Name, model.Order, model.Color, model.IsDefault);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, VacancyStateFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateVacancyStateRequest(id, model.Name, model.Order, model.Color, model.IsDefault);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
