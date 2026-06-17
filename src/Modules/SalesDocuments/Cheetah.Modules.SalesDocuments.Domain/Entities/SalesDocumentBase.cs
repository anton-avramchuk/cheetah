using Cheetah.Core.Domain;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.SalesDocuments.DomainEvents;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат коммерческого документа (КП/заказ/счёт — по <see cref="DocType"/>).
/// Шаблонный модуль не инстанцирует его сам: наследник объявляет конкретный
/// <c>sealed class SalesDocument : SalesDocumentBase</c> со своей фабрикой (через <see cref="InitializeCore"/>)
/// и доп. полями (условия оплаты, доставка, реквизиты). Это точка расширяемости сущности.
/// <para>
/// Статус (<see cref="DocumentStatus"/>) участвует в конечном автомате через
/// <see cref="IStateMachineEntity{TState}"/>; переходы валидируются в Application
/// (<c>IStateMachineValidator</c>) + доменными guard'ами по <see cref="DocType"/>. Строки —
/// <see cref="SalesDocumentLine"/> со снимком цены; итоги пересчитываются <see cref="Recalculate"/>.
/// </para>
/// </summary>
public abstract class SalesDocumentBase : AggregateRoot<Guid>,
    IStateMachineEntity<DocumentStatus>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<SalesDocumentLine> _lines = new();

    public DocType DocType { get; private set; }
    public string Number { get; private set; } = string.Empty; // присваивается при выпуске
    public DocumentStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? DealId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Currency { get; private set; } = null!;       // ISO-4217, одна на документ

    public decimal Subtotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }

    public DateTimeOffset? ValidUntil { get; private set; }
    public Guid? PdfFileId { get; private set; }
    public Guid? SourceDocumentId { get; private set; }
    public string? Attributes { get; private set; }              // jsonb — «быстрый» карман

    public IReadOnlyList<SalesDocumentLine> Lines => _lines;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public DocumentStatus State => Status;

    protected SalesDocumentBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты нового документа (черновик) и доменное событие создания. Вызывается
    /// фабрикой наследника (замена <c>new</c> абстрактной сущности).
    /// </summary>
    protected void InitializeCore(
        Guid id, DocType docType, Guid customerId, Guid ownerId, string currency,
        Guid? dealId = null, DateTimeOffset? validUntil = null, Guid? sourceDocumentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        Id = id;
        DocType = docType;
        CustomerId = customerId;
        OwnerId = ownerId;
        Currency = currency.Trim().ToUpperInvariant();
        DealId = dealId;
        ValidUntil = validUntil;
        SourceDocumentId = sourceDocumentId;
        Status = DocumentStatus.Draft;
        Number = string.Empty;

        AddDomainEvent(new DocumentCreatedIntegrationEvent(Id, (int)docType, customerId, ownerId));
    }

    // ── Строки (правки разрешены только в Draft) ──────────────────────────────────────────────

    /// <summary>Добавить строку-снимок (наименование и цена фиксируются на момент добавления).</summary>
    public virtual void AddLine(Guid productId, string name, decimal unitPrice, decimal qty,
        decimal discountPercent, decimal taxRate)
    {
        EnsureDraft();
        _lines.Add(SalesDocumentLine.Create(Id, productId, name, unitPrice, qty, discountPercent, taxRate));
        Recalculate();
    }

    public virtual void RemoveLine(Guid lineId)
    {
        EnsureDraft();
        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
            return;
        _lines.Remove(line);
        Recalculate();
    }

    /// <summary>Пересчёт итогов из строк. Вызывается при любом изменении состава строк.</summary>
    protected void Recalculate()
    {
        Subtotal = _lines.Sum(l => l.LineSubtotal);
        DiscountTotal = _lines.Sum(l => l.DiscountAmount);
        TaxTotal = _lines.Sum(l => l.TaxAmount);
        GrandTotal = Subtotal - DiscountTotal + TaxTotal;
    }

    // ── Шапка ─────────────────────────────────────────────────────────────────────────────────

    public virtual void UpdateHeader(Guid? dealId, DateTimeOffset? validUntil)
    {
        EnsureDraft();
        DealId = dealId;
        ValidUntil = validUntil;
    }

    // ── Жизненный цикл ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Выпуск документа из черновика: присваивает номер и переводит в выпущенный статус по
    /// <see cref="DocType"/> (КП → Sent, заказ → Confirmed, счёт → Issued). Требует ≥1 строки.
    /// </summary>
    public virtual void Issue(string number)
    {
        EnsureDraft();
        if (_lines.Count == 0)
            throw new InvalidOperationException("Document must have at least one line to be issued.");
        ArgumentException.ThrowIfNullOrWhiteSpace(number);

        Number = number.Trim();
        Status = DocType switch
        {
            DocType.Quote => DocumentStatus.Sent,
            DocType.Order => DocumentStatus.Confirmed,
            DocType.Invoice => DocumentStatus.Issued,
            _ => throw new InvalidOperationException($"Unknown document type {DocType}.")
        };

        AddDomainEvent(new DocumentSentIntegrationEvent(Id, (int)DocType, Number, CustomerId, GrandTotal, Currency));
    }

    /// <summary>КП принято клиентом (Sent → Accepted).</summary>
    public virtual void Accept()
    {
        EnsureDocType(DocType.Quote);
        Status = DocumentStatus.Accepted;
        AddDomainEvent(new QuoteAcceptedIntegrationEvent(Id, DealId, CustomerId, GrandTotal, Currency));
    }

    /// <summary>КП отклонено клиентом (Sent → Rejected).</summary>
    public virtual void Reject(string? reason)
    {
        EnsureDocType(DocType.Quote);
        Status = DocumentStatus.Rejected;
        AddDomainEvent(new QuoteRejectedIntegrationEvent(Id, reason?.Trim()));
    }

    /// <summary>Счёт оплачен (Issued/Overdue → Paid).</summary>
    public virtual void MarkPaid()
    {
        EnsureDocType(DocType.Invoice);
        Status = DocumentStatus.Paid;
        AddDomainEvent(new InvoicePaidIntegrationEvent(Id, CustomerId, GrandTotal, Currency));
    }

    /// <summary>Счёт просрочен (Issued → Overdue). Идемпотентен; вызывается фоновым сканом.</summary>
    public virtual void MarkOverdue()
    {
        if (DocType != DocType.Invoice || Status != DocumentStatus.Issued)
            return;
        Status = DocumentStatus.Overdue;
        AddDomainEvent(new InvoiceOverdueIntegrationEvent(Id, CustomerId, GrandTotal, Currency));
    }

    /// <summary>Аннулировать документ (запрещено для оплаченных/уже аннулированных).</summary>
    public virtual void Cancel(string? reason)
    {
        if (Status is DocumentStatus.Paid or DocumentStatus.Cancelled)
            throw new InvalidOperationException($"Document {Id} cannot be cancelled ({Status}).");
        Status = DocumentStatus.Cancelled;
        AddDomainEvent(new DocumentCancelledIntegrationEvent(Id, reason?.Trim()));
    }

    public void AttachPdf(Guid fileId) => PdfFileId = fileId;

    public void SetAttributes(string? attributes) => Attributes = attributes;

    private void EnsureDraft()
    {
        if (Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Lines/header are editable only in Draft.");
    }

    private void EnsureDocType(DocType type)
    {
        if (DocType != type)
            throw new InvalidOperationException($"Operation is valid only for {type} documents.");
    }
}
