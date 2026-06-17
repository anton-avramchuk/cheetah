using Cheetah.Core.Domain;

namespace Cheetah.Modules.Catalog.Domain.Entities;

/// <summary>
/// Категория товара — узел дерева. Конкретный (не расширяемый) агрегат: дерево категорий редко
/// требует доменного расширения. Иерархия — через <see cref="ParentId"/> + материализованный путь
/// <see cref="Path"/> (идентификаторы предков), что даёт быструю выборку поддерева
/// (<c>Path LIKE '{parent.Path}/%'</c>).
/// </summary>
public sealed class ProductCategory : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public string Path { get; private set; } = null!;
    public int Order { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private ProductCategory() { } // EF

    public static ProductCategory Create(string name, ProductCategory? parent, int order = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ParentId = parent?.Id,
            Order = order
        };
        category.Path = parent is null ? $"/{category.Id}" : $"{parent.Path}/{category.Id}";
        return category;
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void Reorder(int order) => Order = order;
}
