using Cheetah.Contracts.Requests;
using Cheetah.Core.Domain;
using Cheetah.Core.Grid;

namespace Cheetah.AspNetCore.Blazor.Grid;

public abstract class
    BaseCrudService<TEntity, TGridViewModel, TDetailsViewModel, TCreateViewModel> : ICrudService<TGridViewModel
    , TDetailsViewModel, TCreateViewModel> where TGridViewModel : class
    where TDetailsViewModel : class
    where TCreateViewModel : class
    where TEntity : Entity<Guid>
{
    protected readonly IGridRepository<TEntity> Repository;

    protected BaseCrudService(IGridRepository<TEntity> repository)
    {
        Repository = repository;
    }

    public virtual async Task<CrmGridResult<TGridViewModel>> GetGridAsync(CrmPageRequest request,
        CancellationToken ct = default)
    {
        var result = await Repository.GetGridAsync<TGridViewModel>(new GridRequest
        {
            Page = request.Page,
            PageSize = request.PageSize
        }, ct);

        return new CrmGridResult<TGridViewModel>()
        {
            Data = result.Data,
            Total = result.Total
        };
    }

    public virtual async Task<TDetailsViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Repository.GetByIdAsync<TDetailsViewModel>(id, ct);
    }

    public abstract Task CreateAsync(TCreateViewModel model, CancellationToken ct = default);

    public abstract Task UpdateAsync(Guid id, TDetailsViewModel model, CancellationToken ct = default);

    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await Repository.GetByIdAsync(id, ct);
        if (e is null) return;
        Repository.Delete(e);
        await Repository.SaveChangesAsync(ct);
    }
}