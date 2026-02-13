using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Frontend.Models;

namespace Crm.Recruitment.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<StackItemGridViewModel, StackItemFormModel, StackItemFormModel>))]
public sealed class StackItemCrudService : ICrudService<StackItemGridViewModel, StackItemFormModel, StackItemFormModel>
{
    private readonly IStackItemsService _service;

    public StackItemCrudService(IStackItemsService service)
    {
        _service = service;
    }

    public async Task<GridResult<StackItemGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(StackItemGridViewModel.FromResponse).ToList();
        return new GridResult<StackItemGridViewModel>(mapped, mapped.Count);
    }

    public async Task<StackItemFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new StackItemFormModel
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public async Task<Guid> CreateAsync(StackItemFormModel model, CancellationToken ct = default)
    {
        var request = new CreateStackItemRequest(model.Name);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, StackItemFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateStackItemRequest(id, model.Name);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
