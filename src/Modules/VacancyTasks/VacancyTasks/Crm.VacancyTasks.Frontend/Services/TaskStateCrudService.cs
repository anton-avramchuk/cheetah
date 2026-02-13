using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.VacancyTasks.ApiClient;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Frontend.Models;

namespace Crm.VacancyTasks.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<TaskStateGridViewModel, TaskStateFormModel, TaskStateFormModel>))]
public sealed class TaskStateCrudService : ICrudService<TaskStateGridViewModel, TaskStateFormModel, TaskStateFormModel>
{
    private readonly ITaskStatesService _service;

    public TaskStateCrudService(ITaskStatesService service)
    {
        _service = service;
    }

    public async Task<GridResult<TaskStateGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(TaskStateGridViewModel.FromResponse).ToList();
        return new GridResult<TaskStateGridViewModel>(mapped, mapped.Count);
    }

    public async Task<TaskStateFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new TaskStateFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Order = entity.Order,
            Color = entity.Color,
            IsDefault = entity.IsDefault
        };
    }

    public async Task<Guid> CreateAsync(TaskStateFormModel model, CancellationToken ct = default)
    {
        var request = new CreateTaskStateRequest(model.Name, model.Order, model.Color, model.IsDefault);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, TaskStateFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateTaskStateRequest(id, model.Name, model.Order, model.Color, model.IsDefault);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
