using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using __Prefix__.ModuleName.ApiClient;
using __Prefix__.ModuleName.Contracts.Requests;
using __Prefix__.ModuleName.Frontend.Models;

namespace __Prefix__.ModuleName.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<SampleEntityGridViewModel, SampleEntityFormModel, SampleEntityFormModel>))]
public sealed class SampleEntityCrudService : ICrudService<SampleEntityGridViewModel, SampleEntityFormModel, SampleEntityFormModel>
{
    private readonly ISampleEntitiesService _service;

    public SampleEntityCrudService(ISampleEntitiesService service)
    {
        _service = service;
    }

    public async Task<GridResult<SampleEntityGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(SampleEntityGridViewModel.FromResponse).ToList();
        return new GridResult<SampleEntityGridViewModel>(mapped, mapped.Count);
    }

    public async Task<SampleEntityFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new SampleEntityFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description ?? ""
        };
    }

    public async Task<Guid> CreateAsync(SampleEntityFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new CreateSampleEntityRequest(model.Name, description);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, SampleEntityFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new UpdateSampleEntityRequest(id, model.Name, description);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
