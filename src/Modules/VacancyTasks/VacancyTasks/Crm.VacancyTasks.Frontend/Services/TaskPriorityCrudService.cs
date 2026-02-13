using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.ApiClient;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Frontend.Models;

namespace Crm.VacancyTasks.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<TaskPriorityGridViewModel, TaskPriorityFormModel, TaskPriorityFormModel>))]
public sealed class TaskPriorityCrudService : ICrudService<TaskPriorityGridViewModel, TaskPriorityFormModel, TaskPriorityFormModel>
{
    private readonly ITaskPrioritiesService _service;

    public TaskPriorityCrudService(ITaskPrioritiesService service)
    {
        _service = service;
    }

    public async Task<GridResult<TaskPriorityGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(TaskPriorityGridViewModel.FromResponse).ToList();
        return new GridResult<TaskPriorityGridViewModel>(mapped, mapped.Count);
    }

    public async Task<TaskPriorityFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new TaskPriorityFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Order = entity.Order,
            Color = entity.Color
        };
    }

    public async Task<Guid> CreateAsync(TaskPriorityFormModel model, CancellationToken ct = default)
    {
        var request = new CreateTaskPriorityRequest(model.Name, model.Order, model.Color);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, TaskPriorityFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateTaskPriorityRequest(id, model.Name, model.Order, model.Color);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
