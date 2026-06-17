using Cheetah.Core.Domain;

namespace Cheetah.Modules.SalesDocuments.Domain.Entities;

/// <summary>
/// Строка коммерческого документа — дитя агрегата <see cref="SalesDocumentBase"/>. Хранит снимок
/// наименования и цены товара на момент добавления, поэтому последующие правки каталога документ не
/// меняют. Создаётся/удаляется только через агрегат, поэтому фабрика — <c>internal</c>. Суммы строки
/// (<see cref="LineTotal"/> и др.) вычисляемые.
/// </summary>
public sealed class SalesDocumentLine : Entity<Guid>
{
    public Guid DocumentId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;     // снимок наименования
    public decimal UnitPrice { get; private set; }        // снимок цены из Catalog
    public decimal Qty { get; private set; }
    public decimal DiscountPercent { get; private set; }  // 0..100
    public decimal TaxRate { get; private set; }          // 0..100

    public decimal LineSubtotal => UnitPrice * Qty;
    public decimal DiscountAmount => LineSubtotal * DiscountPercent / 100m;
    public decimal TaxAmount => (LineSubtotal - DiscountAmount) * TaxRate / 100m;
    public decimal LineTotal => LineSubtotal - DiscountAmount + TaxAmount;

    private SalesDocumentLine() { } // EF

    internal static SalesDocumentLine Create(
        Guid documentId, Guid productId, string name, decimal unitPrice, decimal qty,
        decimal discountPercent, decimal taxRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be non-negative.");
        if (qty <= 0)
            throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be positive.");
        if (discountPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercent), "Discount must be in 0..100.");
        if (taxRate is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(taxRate), "Tax rate must be in 0..100.");

        return new SalesDocumentLine
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ProductId = productId,
            Name = name.Trim(),
            UnitPrice = unitPrice,
            Qty = qty,
            DiscountPercent = discountPercent,
            TaxRate = taxRate
        };
    }
}
