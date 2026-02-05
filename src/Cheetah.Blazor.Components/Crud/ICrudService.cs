using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;

namespace Cheetah.Blazor.Components.Crud;

/// <summary>
/// Generic interface for CRUD operations.
/// </summary>
/// <typeparam name="TGridViewModel">View model for grid display.</typeparam>
/// <typeparam name="TCreateViewModel">View model for create form.</typeparam>
/// <typeparam name="TEditViewModel">View model for edit form.</typeparam>
public interface ICrudService<TGridViewModel, TCreateViewModel, TEditViewModel>
    where TGridViewModel : IGridViewModel
{
    /// <summary>
    /// Gets items with pagination, sorting and filtering.
    /// </summary>
    Task<GridResult<TGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets item by id for editing.
    /// </summary>
    Task<TEditViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new item.
    /// </summary>
    Task<Guid> CreateAsync(TCreateViewModel model, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing item.
    /// </summary>
    Task UpdateAsync(Guid id, TEditViewModel model, CancellationToken ct = default);

    /// <summary>
    /// Deletes an item by id.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
