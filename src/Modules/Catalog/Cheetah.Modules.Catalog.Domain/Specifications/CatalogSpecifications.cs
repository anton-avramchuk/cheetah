using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Domain.Specifications;

/// <summary>Прайс-лист по идентификатору (для загрузки агрегата вместе со строками через Include).</summary>
public sealed class PriceListByIdSpecification : Specification<PriceList>
{
    private readonly Guid _id;

    public PriceListByIdSpecification(Guid id) => _id = id;

    public override Expression<Func<PriceList, bool>> ToExpression()
        => pl => pl.Id == _id;
}

/// <summary>Прайс-лист «по умолчанию» (используется при разрешении цены без явного прайса).</summary>
public sealed class DefaultPriceListSpecification : Specification<PriceList>
{
    public override Expression<Func<PriceList, bool>> ToExpression()
        => pl => pl.IsDefault;
}

/// <summary>Категории по родителю (null = корневые). Сортировка применяется на стороне запроса.</summary>
public sealed class CategoriesByParentSpecification : Specification<ProductCategory>
{
    private readonly Guid? _parentId;

    public CategoriesByParentSpecification(Guid? parentId) => _parentId = parentId;

    public override Expression<Func<ProductCategory, bool>> ToExpression()
        => c => c.ParentId == _parentId;
}
