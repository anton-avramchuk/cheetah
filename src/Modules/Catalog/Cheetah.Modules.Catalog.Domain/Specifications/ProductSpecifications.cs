using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Domain.Specifications;

/// <summary>Товар по артикулу (Sku). Используется для проверки уникальности при создании.</summary>
public sealed class ProductBySkuSpecification<TProduct> : Specification<TProduct>
    where TProduct : ProductBase
{
    private readonly string _sku;

    public ProductBySkuSpecification(string sku) => _sku = sku;

    public override Expression<Func<TProduct, bool>> ToExpression()
        => p => p.Sku == _sku;
}

/// <summary>Активные товары указанной категории.</summary>
public sealed class ActiveProductsByCategorySpecification<TProduct> : Specification<TProduct>
    where TProduct : ProductBase
{
    private readonly Guid _categoryId;

    public ActiveProductsByCategorySpecification(Guid categoryId) => _categoryId = categoryId;

    public override Expression<Func<TProduct, bool>> ToExpression()
        => p => p.IsActive && p.CategoryId == _categoryId;
}

/// <summary>
/// Комбинированный фильтр списка товаров. Любой критерий опционален (null = не учитывать). Поиск —
/// по подстроке в имени или артикуле. Используется generic query-handler'ом вместо raw LINQ.
/// </summary>
public sealed class ProductsFilterSpecification<TProduct> : Specification<TProduct>
    where TProduct : ProductBase
{
    private readonly string? _search;
    private readonly Guid? _categoryId;
    private readonly bool _onlyActive;

    public ProductsFilterSpecification(string? search, Guid? categoryId, bool onlyActive)
    {
        _search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        _categoryId = categoryId;
        _onlyActive = onlyActive;
    }

    public override Expression<Func<TProduct, bool>> ToExpression()
        => p => (!_onlyActive || p.IsActive)
                && (_categoryId == null || p.CategoryId == _categoryId)
                && (_search == null || p.Name.Contains(_search) || p.Sku.Contains(_search));
}
