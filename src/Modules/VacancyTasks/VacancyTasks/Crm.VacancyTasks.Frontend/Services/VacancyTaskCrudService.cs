using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.ApiClient;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Frontend.Models;

namespace Crm.VacancyTasks.Frontend.Services;

/// <summary>
/// CRUD service adapter for VacancyTask.
/// </summary>
[Export(LifetimeType.Scoped,
    typeof(ICrudService<VacancyTaskGridViewModel, VacancyTaskFormModel, VacancyTaskFormModel>))]
public sealed class
    VacancyTaskCrudService : ICrudService<VacancyTaskGridViewModel, VacancyTaskFormModel, VacancyTaskFormModel>
{
    private readonly IVacancyTasksService _service;

    public VacancyTaskCrudService(IVacancyTasksService service)
    {
        _service = service;
    }

    public async Task<GridResult<VacancyTaskGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(null, ct);
        var mapped = result.Data.Select(VacancyTaskGridViewModel.FromResponse).ToList();
        return new GridResult<VacancyTaskGridViewModel>(mapped, result.Total);
    }

    public async Task<VacancyTaskFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new VacancyTaskFormModel
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description ?? "",
            VacancyId = entity.VacancyId,
            StateId = entity.StateId,
            PriorityId = entity.PriorityId,
            AssigneeId = entity.AssigneeId,
            DueDate = entity.DueDate
        };
    }

    public async Task<Guid> CreateAsync(VacancyTaskFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new CreateVacancyTaskRequest(model.Title, model.VacancyId, model.StateId, description, model.PriorityId, model.AssigneeId, model.DueDate);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, VacancyTaskFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new UpdateVacancyTaskRequest(id, model.Title, description);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
