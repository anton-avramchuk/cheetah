using Cheetah.Blazor.Components.Crud;
using Cheetah.Core.DependencyInjection;
using __Prefix__.ModuleName.ApiClient;
using __Prefix__.ModuleName.Contracts.Requests;
using __Prefix__.ModuleName.Frontend.Models;

namespace __Prefix__.ModuleName.Frontend.Services;

/// <summary>
/// CRUD service adapter for SampleEntity.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICrudService<SampleEntityGridViewModel, SampleEntityFormModel, SampleEntityFormModel>))]
public sealed class SampleEntityCrudService : ICrudService<SampleEntityGridViewModel, SampleEntityFormModel, SampleEntityFormModel>
{
    private readonly IModuleNameService _service;

    public SampleEntityCrudService(IModuleNameService service)
    {
        _service = service;
    }

    public async Task<IReadOnlyList<SampleEntityGridViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _service.GetAllAsync(ct);
        return entities.Select(SampleEntityGridViewModel.FromResponse).ToList();
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
