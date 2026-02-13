using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Frontend.Models;

namespace Crm.Recruitment.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<PositionGridViewModel, PositionFormModel, PositionFormModel>))]
public sealed class PositionCrudService : ICrudService<PositionGridViewModel, PositionFormModel, PositionFormModel>
{
    private readonly IPositionsService _service;

    public PositionCrudService(IPositionsService service)
    {
        _service = service;
    }

    public async Task<GridResult<PositionGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(PositionGridViewModel.FromResponse).ToList();
        return new GridResult<PositionGridViewModel>(mapped, mapped.Count);
    }

    public async Task<PositionFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new PositionFormModel
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public async Task<Guid> CreateAsync(PositionFormModel model, CancellationToken ct = default)
    {
        var request = new CreatePositionRequest(model.Name);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, PositionFormModel model, CancellationToken ct = default)
    {
        var request = new UpdatePositionRequest(id, model.Name);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
