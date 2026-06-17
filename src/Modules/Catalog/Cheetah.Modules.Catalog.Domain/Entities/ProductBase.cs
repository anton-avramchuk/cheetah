using Cheetah.Core.Domain;
using Cheetah.Modules.Catalog.DomainEvents;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат номенклатуры (товар/услуга). Шаблонный модуль не инстанцирует его
/// сам — наследник объявляет конкретный <c>sealed class Product : ProductBase</c> со своей фабрикой
/// (через <see cref="InitializeCore"/>) и доп. полями (бренд, штрихкод, вес…). Это и есть точка
/// расширяемости сущности.
/// <para>
/// Цена на товаре не хранится — она задаётся в прайс-листах (<see cref="PriceList"/>): одна
/// номенклатура может иметь разные цены для валют/сегментов. Деактивация — через <see cref="IsActive"/>.
/// </para>
/// </summary>
public abstract class ProductBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public ProductType Type { get; private set; }
    public UnitOfMeasure Unit { get; private set; }
    public Guid? CategoryId { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>«Быстрый» карман расширения без миграций (jsonb). Полноценно — модуль Custom Fields.</summary>
    public string? Attributes { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected ProductBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты нового товара и доменное событие создания. Вызывается фабрикой наследника
    /// (замена <c>new</c> абстрактной сущности).
    /// </summary>
    protected void InitializeCore(
        Guid id, string sku, string name, ProductType type, UnitOfMeasure unit,
        Guid? categoryId = null, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Sku = sku.Trim();
        Name = name.Trim();
        Type = type;
        Unit = unit;
        CategoryId = categoryId;
        Description = description;
        IsActive = true;

        AddDomainEvent(new ProductCreatedIntegrationEvent(Id, Sku, Name, (int)Type));
    }

    /// <summary>Обновление базовых полей. Доп. поля наследника обновляет он сам (переопределив хендлер).</summary>
    public virtual void Update(string name, string? description, Guid? categoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Description = description;
        CategoryId = categoryId;
        AddDomainEvent(new ProductUpdatedIntegrationEvent(Id));
    }

    public virtual void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new ProductDeactivatedIntegrationEvent(Id));
    }

    public virtual void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AddDomainEvent(new ProductActivatedIntegrationEvent(Id));
    }

    public void SetAttributes(string? attributes) => Attributes = attributes;
}
