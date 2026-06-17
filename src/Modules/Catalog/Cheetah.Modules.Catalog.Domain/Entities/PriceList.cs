using Cheetah.Core.Domain;
using Cheetah.Modules.Catalog.DomainEvents;

namespace Cheetah.Modules.Catalog.Domain.Entities;

/// <summary>
/// Прайс-лист — агрегат заголовка (валюта, период действия, флаг «по умолчанию») со строками цен
/// (<see cref="PriceListItem"/>). Конкретный (не расширяемый): заголовок прайса стабилен. Цена в
/// прайс-листе — источник истины для разрешения цены товара при количестве.
/// </summary>
public sealed class PriceList : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<PriceListItem> _items = new();

    public string Name { get; private set; } = null!;
    public string Currency { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }

    public IReadOnlyList<PriceListItem> Items => _items;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private PriceList() { } // EF

    public static PriceList Create(
        string name, string currency, bool isDefault = false,
        DateTimeOffset? validFrom = null, DateTimeOffset? validTo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new PriceList
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),
            IsDefault = isDefault,
            ValidFrom = validFrom,
            ValidTo = validTo
        };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void ChangeValidity(DateTimeOffset? validFrom, DateTimeOffset? validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public void SetDefault(bool isDefault) => IsDefault = isDefault;

    /// <summary>Устанавливает или изменяет цену позиции (по паре товар + порог количества).</summary>
    public void SetPrice(Guid productId, decimal price, int? minQty = null)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.MinQty == minQty);
        if (item is null)
            _items.Add(PriceListItem.Create(Id, productId, price, minQty));
        else
            item.ChangePrice(price);

        AddDomainEvent(new PriceChangedIntegrationEvent(Id, productId, price, Currency));
    }

    /// <summary>
    /// Разрешает цену товара при заданном количестве: берёт строку с наибольшим подходящим порогом
    /// <c>MinQty</c> (или без порога). Возвращает <c>null</c>, если цены для товара в прайсе нет.
    /// </summary>
    public decimal? ResolvePrice(Guid productId, int qty)
        => _items
            .Where(i => i.ProductId == productId && (i.MinQty == null || qty >= i.MinQty))
            .OrderByDescending(i => i.MinQty ?? 0)
            .Select(i => (decimal?)i.Price)
            .FirstOrDefault();
}
