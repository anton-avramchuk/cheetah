using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Catalog.Blazor.ViewModels;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Blazor.Services;

/// <summary>
/// CRUD категорий для грида: чтение/удаление — из <see cref="BaseCrudService{TEntity,TGrid,TDetails,TCreate}"/>
/// поверх <see cref="IGridRepository{TEntity}"/>; создание/обновление — через доменные фабрики
/// <see cref="ProductCategory"/>.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICrudService<CategoryGridViewModel, CategoryDetailsViewModel, CategoryCreateViewModel>))]
public sealed class CategoryCrudService(IGridRepository<ProductCategory> repository)
    : BaseCrudService<ProductCategory, CategoryGridViewModel, CategoryDetailsViewModel, CategoryCreateViewModel>(repository)
{
    public override async Task CreateAsync(CategoryCreateViewModel model, CancellationToken ct = default)
    {
        var category = ProductCategory.Create(model.Name, parent: null, model.Order);
        Repository.Add(category);
        await Repository.SaveChangesAsync(ct);
    }

    public override async Task UpdateAsync(Guid id, CategoryDetailsViewModel model, CancellationToken ct = default)
    {
        var category = await Repository.GetByIdAsync(id, ct);
        if (category is null) return;

        category.Rename(model.Name);
        category.Reorder(model.Order);
        await Repository.SaveChangesAsync(ct);
    }
}
