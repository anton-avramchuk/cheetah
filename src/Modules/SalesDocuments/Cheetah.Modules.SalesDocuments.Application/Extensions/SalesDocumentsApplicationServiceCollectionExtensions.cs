using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.SalesDocuments.Application.Abstractions;
using Cheetah.Modules.SalesDocuments.Application.Documents;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.SalesDocuments.Application.Extensions;

public static class SalesDocumentsApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы конкретной реализации
    /// SalesDocuments. Вызывается из прикладного модуля наследника после
    /// <c>AddSalesDocumentsInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddSalesDocumentsApplication<
        TDoc, TCreateRequest, TUpdateRequest, TDto, TGridViewModel, TFactory, TProjector>(
        this IServiceCollection services)
        where TDoc : SalesDocumentBase
        where TCreateRequest : CreateDocumentRequestBase
        where TUpdateRequest : UpdateDocumentRequestBase
        where TDto : SalesDocumentDtoBase
        where TGridViewModel : SalesDocumentGridViewModelBase
        where TFactory : class, ISalesDocumentFactory<TDoc, TCreateRequest>
        where TProjector : class, ISalesDocumentProjector<TDoc, TDto>
    {
        services.AddScoped<ISalesDocumentFactory<TDoc, TCreateRequest>, TFactory>();
        services.AddScoped<ISalesDocumentProjector<TDoc, TDto>, TProjector>();

        // Команды с результатом
        services.AddScoped<ICommandHandler<CreateDocumentCommand<TCreateRequest>, Guid>,
            CreateDocumentCommandHandler<TDoc, TCreateRequest>>();
        services.AddScoped<ICommandHandler<ConvertDocumentCommand, Guid>,
            ConvertDocumentCommandHandler<TDoc, TCreateRequest>>();
        services.AddScoped<ICommandHandler<GenerateDocumentPdfCommand, Guid>,
            GenerateDocumentPdfCommandHandler<TDoc>>();

        // Команды без результата
        services.AddScoped<ICommandHandler<UpdateDocumentCommand<TUpdateRequest>>,
            UpdateDocumentCommandHandler<TDoc, TUpdateRequest>>();
        services.AddScoped<ICommandHandler<AddLineCommand>, AddLineCommandHandler<TDoc>>();
        services.AddScoped<ICommandHandler<RemoveLineCommand>, RemoveLineCommandHandler<TDoc>>();
        services.AddScoped<ICommandHandler<IssueDocumentCommand>, IssueDocumentCommandHandler<TDoc>>();
        services.AddScoped<ICommandHandler<AcceptQuoteCommand>, AcceptQuoteCommandHandler<TDoc>>();
        services.AddScoped<ICommandHandler<RejectQuoteCommand>, RejectQuoteCommandHandler<TDoc>>();
        services.AddScoped<ICommandHandler<MarkInvoicePaidCommand>, MarkInvoicePaidCommandHandler<TDoc>>();
        services.AddScoped<ICommandHandler<CancelDocumentCommand>, CancelDocumentCommandHandler<TDoc>>();

        // Запросы
        services.AddScoped<IQueryHandler<GetDocumentByIdQuery<TDto>, TDto?>,
            GetDocumentByIdQueryHandler<TDoc, TDto>>();
        services.AddScoped<IQueryHandler<ListDocumentsQuery<TDto>, IReadOnlyList<TDto>>,
            ListDocumentsQueryHandler<TDoc, TDto>>();
        services.AddScoped<IQueryHandler<GetDocumentsGridQuery<TGridViewModel>, GridResult<TGridViewModel>>,
            GetDocumentsGridQueryHandler<TDoc, TGridViewModel>>();

        return services;
    }
}
