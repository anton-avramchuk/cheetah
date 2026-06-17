using Cheetah.Core.Domain;

namespace Cheetah.Modules.Catalog.Domain.Entities;

/// <summary>
/// Строка прайс-листа — дитя агрегата <see cref="PriceList"/>. Хранит цену товара, опционально
/// действующую от порога количества (<see cref="MinQty"/>). Создаётся/изменяется только через
/// агрегат, поэтому фабрика и мутатор — <c>internal</c>.
/// </summary>
public sealed class PriceListItem : Entity<Guid>
{
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }
    public int? MinQty { get; private set; }

    private PriceListItem() { } // EF

    internal static PriceListItem Create(Guid priceListId, Guid productId, decimal price, int? minQty)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be non-negative.");
        if (minQty is < 1)
            throw new ArgumentOutOfRangeException(nameof(minQty), "MinQty must be positive.");

        return new PriceListItem
        {
            Id = Guid.NewGuid(),
            PriceListId = priceListId,
            ProductId = productId,
            Price = price,
            MinQty = minQty
        };
    }

    internal void ChangePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be non-negative.");
        Price = price;
    }
}
