using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Backend.Endpoints.Responses;
using Cheetah.Modules.SalesDocuments.Application.Documents;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Api.Endpoints;

/// <summary>
/// Конкретные эндпоинты операций над документом (строки + жизненный цикл). Команды не зависят от
/// конкретного типа документа (работают через закрытые наследником generic-handler'ы), поэтому
/// эндпоинты самодостаточны («из коробки»). Маппинг Request→Command — в <c>SalesDocumentsMappingProfile</c>.
/// </summary>
public sealed class AddLineEndpoint : CommandEndpoint<AddLineToDocumentRequest, AddLineCommand>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/lines";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class RemoveLineEndpoint : DeleteCommandEndpoint<RemoveLineRequest, RemoveLineCommand>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/lines/{{lineId:guid}}";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class IssueDocumentEndpoint : CommandEndpoint<IssueDocumentRequest, IssueDocumentCommand>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/issue";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class AcceptQuoteEndpoint : CommandEndpoint<AcceptQuoteRequest, AcceptQuoteCommand>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/accept";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class RejectQuoteEndpoint : CommandEndpoint<RejectQuoteRequest, RejectQuoteCommand>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/reject";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class MarkInvoicePaidEndpoint : CommandEndpoint<MarkInvoicePaidRequest, MarkInvoicePaidCommand>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/pay";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class CancelDocumentEndpoint : CommandEndpoint<CancelDocumentRequest, CancelDocumentCommand>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/cancel";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class ConvertDocumentEndpoint
    : CommandWithResultEndpoint<ConvertDocumentRequest, ConvertDocumentCommand, Guid, GuidResponse>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/convert";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}

public sealed class GenerateDocumentPdfEndpoint
    : CommandWithResultEndpoint<GenerateDocumentPdfRequest, GenerateDocumentPdfCommand, Guid, GuidResponse>
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}/pdf";
    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments.Lifecycle");
}
