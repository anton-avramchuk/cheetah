using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Frontend.Models;

namespace Crm.Recruitment.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<WorkFormatGridViewModel, WorkFormatFormModel, WorkFormatFormModel>))]
public sealed class WorkFormatCrudService : ICrudService<WorkFormatGridViewModel, WorkFormatFormModel, WorkFormatFormModel>
{
    private readonly IWorkFormatsService _service;

    public WorkFormatCrudService(IWorkFormatsService service)
    {
        _service = service;
    }

    public async Task<GridResult<WorkFormatGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(WorkFormatGridViewModel.FromResponse).ToList();
        return new GridResult<WorkFormatGridViewModel>(mapped, mapped.Count);
    }

    public async Task<WorkFormatFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new WorkFormatFormModel
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public async Task<Guid> CreateAsync(WorkFormatFormModel model, CancellationToken ct = default)
    {
        var request = new CreateWorkFormatRequest(model.Name);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, WorkFormatFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateWorkFormatRequest(id, model.Name);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
