using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Domain.Specifications;

/// <summary>Документ по номеру. Для проверки уникальности номера.</summary>
public sealed class DocumentByNumberSpecification<TDoc> : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    private readonly string _number;

    public DocumentByNumberSpecification(string number) => _number = number;

    public override Expression<Func<TDoc, bool>> ToExpression() => d => d.Number == _number;
}

/// <summary>
/// Комбинированный фильтр списка документов. Любой критерий опционален (null = не учитывать).
/// Используется generic query-handler'ом вместо raw LINQ.
/// </summary>
public sealed class DocumentsFilterSpecification<TDoc> : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    private readonly Guid? _customerId;
    private readonly DocType? _type;
    private readonly DocumentStatus? _status;

    public DocumentsFilterSpecification(Guid? customerId, DocType? type, DocumentStatus? status)
    {
        _customerId = customerId;
        _type = type;
        _status = status;
    }

    public override Expression<Func<TDoc, bool>> ToExpression()
        => d => (_customerId == null || d.CustomerId == _customerId)
                && (_type == null || d.DocType == _type)
                && (_status == null || d.Status == _status);
}

/// <summary>Черновики КП, содержащие указанный товар. Для пометки «цена устарела» по PriceChanged.</summary>
public sealed class DraftQuotesByProductSpecification<TDoc> : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    private readonly Guid _productId;

    public DraftQuotesByProductSpecification(Guid productId) => _productId = productId;

    public override Expression<Func<TDoc, bool>> ToExpression()
        => d => d.DocType == DocType.Quote && d.Status == DocumentStatus.Draft
                && d.Lines.Any(l => l.ProductId == _productId);
}

/// <summary>Выпущенные счета с истёкшим сроком оплаты (для фонового скана просрочки).</summary>
public sealed class OverdueInvoicesSpecification<TDoc> : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    private readonly DateTimeOffset _asOf;

    public OverdueInvoicesSpecification(DateTimeOffset asOf) => _asOf = asOf;

    public override Expression<Func<TDoc, bool>> ToExpression()
        => d => d.DocType == DocType.Invoice && d.Status == DocumentStatus.Issued
                && d.ValidUntil != null && d.ValidUntil < _asOf;
}
