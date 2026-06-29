namespace Cheetah.AspNetCore.Blazor.Grid;

public interface ICrudService<TGridViewModel, TDetailsViewModel, TCreateViewModel>
    where TGridViewModel : class
    where TDetailsViewModel : class
    where TCreateViewModel : class
{
    Task<CrmGridResult<TGridViewModel>> GetGridAsync(CrmPageRequest request, CancellationToken ct = default);
    Task<TDetailsViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Создаёт запись и возвращает её идентификатор.</summary>
    Task<Guid> CreateAsync(TCreateViewModel model, CancellationToken ct = default);
    Task UpdateAsync(Guid id, TDetailsViewModel model, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}