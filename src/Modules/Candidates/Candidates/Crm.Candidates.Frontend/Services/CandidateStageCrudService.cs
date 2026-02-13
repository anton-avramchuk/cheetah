using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.ApiClient;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Frontend.Models;

namespace Crm.Candidates.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<CandidateStageGridViewModel, CandidateStageFormModel, CandidateStageFormModel>))]
public sealed class CandidateStageCrudService : ICrudService<CandidateStageGridViewModel, CandidateStageFormModel, CandidateStageFormModel>
{
    private readonly ICandidateStagesService _service;

    public CandidateStageCrudService(ICandidateStagesService service)
    {
        _service = service;
    }

    public async Task<GridResult<CandidateStageGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(CandidateStageGridViewModel.FromResponse).ToList();
        return new GridResult<CandidateStageGridViewModel>(mapped, mapped.Count);
    }

    public async Task<CandidateStageFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new CandidateStageFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Order = entity.Order,
            Color = entity.Color,
            IsDefault = entity.IsDefault
        };
    }

    public async Task<Guid> CreateAsync(CandidateStageFormModel model, CancellationToken ct = default)
    {
        var request = new CreateCandidateStageRequest(model.Name, model.Order, model.Color, model.IsDefault);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, CandidateStageFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateCandidateStageRequest(id, model.Name, model.Order, model.Color, model.IsDefault);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
