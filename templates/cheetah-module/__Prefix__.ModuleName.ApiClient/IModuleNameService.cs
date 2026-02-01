using __Prefix__.ModuleName.Contracts.Requests;
using __Prefix__.ModuleName.Contracts.Response;

namespace __Prefix__.ModuleName.ApiClient;

public interface IModuleNameService
{
    ValueTask<IReadOnlyList<SampleEntityViewModel>> GetAllAsync(CancellationToken ct = default);
    ValueTask<SampleEntityViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateSampleEntityRequest request, CancellationToken ct = default);
    ValueTask UpdateAsync(Guid id, UpdateSampleEntityRequest request, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}
