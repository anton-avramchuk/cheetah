using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;

namespace Cheetah.Blazor.Components.Crud;

/// <summary>
/// Minimal service interface for grid operations (list + delete).
/// Used by <see cref="CrmNavCrudGrid{TGridViewModel}"/> which navigates to separate pages for create/edit.
/// </summary>
/// <typeparam name="TGridViewModel">View model for grid display.</typeparam>
public interface IGridService<TGridViewModel> where TGridViewModel : IGridViewModel
{
    /// <summary>
    /// Gets items with pagination, sorting and filtering.
    /// </summary>
    Task<GridResult<TGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes an item by id.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
