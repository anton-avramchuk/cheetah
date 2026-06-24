using Cheetah.Mapping.Core;
using Cheetah.Modules.SalesDocuments.Application.Documents;
using Cheetah.Modules.SalesDocuments.Contracts;

namespace Cheetah.Modules.SalesDocuments.Mapping;

/// <summary>
/// Реестр генерируемых мапперов SalesDocuments (аналог Mapster-профиля, но через source generator).
///
/// Особенность модуля: у каждого реквеста id приходит из роута как <c>Id</c>, а команда ждёт
/// <c>DocumentId</c>. Переименование объявлено здесь же через <see cref="MapMemberAttribute"/> —
/// БЕЗ атрибутов на самих командах (Application остаётся чистым). Остальные поля совпадают по имени.
///
/// <c>AddLineToDocumentRequest → AddLineCommand</c> покрыт через <see cref="MapNestedAttribute"/>:
/// <c>DocumentId ← Id</c>, а вложенный <c>Line</c> собирается из плоских полей реквеста в <c>AddLineRequest</c>.
/// </summary>
[GenerateMapper(typeof(AddLineToDocumentRequest), typeof(AddLineCommand), GenerateProjection = false)]
[MapMember(typeof(AddLineCommand), nameof(AddLineCommand.DocumentId), nameof(AddLineToDocumentRequest.Id))]
[MapNested(typeof(AddLineCommand), nameof(AddLineCommand.Line))]

[GenerateMapper(typeof(RemoveLineRequest), typeof(RemoveLineCommand), GenerateProjection = false)]
[MapMember(typeof(RemoveLineCommand), nameof(RemoveLineCommand.DocumentId), nameof(RemoveLineRequest.Id))]

[GenerateMapper(typeof(IssueDocumentRequest), typeof(IssueDocumentCommand), GenerateProjection = false)]
[MapMember(typeof(IssueDocumentCommand), nameof(IssueDocumentCommand.DocumentId), nameof(IssueDocumentRequest.Id))]

[GenerateMapper(typeof(AcceptQuoteRequest), typeof(AcceptQuoteCommand), GenerateProjection = false)]
[MapMember(typeof(AcceptQuoteCommand), nameof(AcceptQuoteCommand.DocumentId), nameof(AcceptQuoteRequest.Id))]

[GenerateMapper(typeof(RejectQuoteRequest), typeof(RejectQuoteCommand), GenerateProjection = false)]
[MapMember(typeof(RejectQuoteCommand), nameof(RejectQuoteCommand.DocumentId), nameof(RejectQuoteRequest.Id))]

[GenerateMapper(typeof(MarkInvoicePaidRequest), typeof(MarkInvoicePaidCommand), GenerateProjection = false)]
[MapMember(typeof(MarkInvoicePaidCommand), nameof(MarkInvoicePaidCommand.DocumentId), nameof(MarkInvoicePaidRequest.Id))]

[GenerateMapper(typeof(CancelDocumentRequest), typeof(CancelDocumentCommand), GenerateProjection = false)]
[MapMember(typeof(CancelDocumentCommand), nameof(CancelDocumentCommand.DocumentId), nameof(CancelDocumentRequest.Id))]

[GenerateMapper(typeof(ConvertDocumentRequest), typeof(ConvertDocumentCommand), GenerateProjection = false)]
[MapMember(typeof(ConvertDocumentCommand), nameof(ConvertDocumentCommand.DocumentId), nameof(ConvertDocumentRequest.Id))]

[GenerateMapper(typeof(GenerateDocumentPdfRequest), typeof(GenerateDocumentPdfCommand), GenerateProjection = false)]
[MapMember(typeof(GenerateDocumentPdfCommand), nameof(GenerateDocumentPdfCommand.DocumentId), nameof(GenerateDocumentPdfRequest.Id))]
public static partial class SalesDocumentsMappingRegistry
{
}
