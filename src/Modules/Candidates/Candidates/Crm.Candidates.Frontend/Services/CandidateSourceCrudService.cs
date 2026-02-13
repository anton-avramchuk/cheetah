using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.ApiClient;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Frontend.Models;

namespace Crm.Candidates.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<CandidateSourceGridViewModel, CandidateSourceFormModel, CandidateSourceFormModel>))]
public sealed class CandidateSourceCrudService : ICrudService<CandidateSourceGridViewModel, CandidateSourceFormModel, CandidateSourceFormModel>
{
    private readonly ICandidateSourcesService _service;

    public CandidateSourceCrudService(ICandidateSourcesService service)
    {
        _service = service;
    }

    public async Task<GridResult<CandidateSourceGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(CandidateSourceGridViewModel.FromResponse).ToList();
        return new GridResult<CandidateSourceGridViewModel>(mapped, mapped.Count);
    }

    public async Task<CandidateSourceFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new CandidateSourceFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Order = entity.Order,
            Color = entity.Color
        };
    }

    public async Task<Guid> CreateAsync(CandidateSourceFormModel model, CancellationToken ct = default)
    {
        var request = new CreateCandidateSourceRequest(model.Name, model.Order, model.Color);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, CandidateSourceFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateCandidateSourceRequest(id, model.Name, model.Order, model.Color);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
