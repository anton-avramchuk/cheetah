using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Frontend.Models;

namespace Crm.Recruitment.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<VacancyGridViewModel, VacancyFormModel, VacancyFormModel>))]
public sealed class VacancyCrudService : ICrudService<VacancyGridViewModel, VacancyFormModel, VacancyFormModel>
{
    private readonly IVacanciesService _service;

    public VacancyCrudService(IVacanciesService service)
    {
        _service = service;
    }

    public async Task<GridResult<VacancyGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(null, ct);
        var mapped = result.Data.Select(VacancyGridViewModel.FromResponse).ToList();
        return new GridResult<VacancyGridViewModel>(mapped, result.Total);
    }

    public async Task<VacancyFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new VacancyFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public async Task<Guid> CreateAsync(VacancyFormModel model, CancellationToken ct = default)
    {
        var request = new CreateVacancyRequest(
            model.Name,
            model.Description);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, VacancyFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateVacancyRequest(
            id,
            model.Name,
            model.Description);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
