using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Candidates.ApiClient;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Frontend.Models;

namespace Crm.Candidates.Frontend.Services;

/// <summary>
/// CRUD service adapter for Candidate.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICrudService<CandidateGridViewModel, CandidateFormModel, CandidateFormModel>))]
public sealed class CandidateCrudService : ICrudService<CandidateGridViewModel, CandidateFormModel, CandidateFormModel>
{
    private readonly ICandidatesService _service;

    public CandidateCrudService(ICandidatesService service)
    {
        _service = service;
    }

    public async Task<GridResult<CandidateGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(null, ct);
        var mapped = result.Data.Select(CandidateGridViewModel.FromResponse).ToList();
        return new GridResult<CandidateGridViewModel>(mapped, result.Total);
    }

    public async Task<CandidateFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new CandidateFormModel
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            City = entity.City,
            CurrentPosition = entity.CurrentPosition,
            CurrentCompany = entity.CurrentCompany,
            SalaryExpectation = entity.SalaryExpectation,
            About = entity.About
        };
    }

    public async Task<Guid> CreateAsync(CandidateFormModel model, CancellationToken ct = default)
    {
        var request = new CreateCandidateRequest(
            model.FirstName,
            model.LastName,
            model.Email,
            model.Phone,
            model.City,
            model.CurrentPosition,
            model.CurrentCompany,
            model.SalaryExpectation,
            model.About);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, CandidateFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateCandidateRequest(
            id,
            model.FirstName,
            model.LastName,
            model.Email,
            model.Phone,
            model.City,
            model.CurrentPosition,
            model.CurrentCompany,
            model.SalaryExpectation,
            model.About);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
