using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Api.Endpoints;

/// <summary>
/// Абстрактные шаблоны эндпоинтов документа (документ расширяем). Наследник/хост закрывает
/// generic-параметры своими конкретными Request/Command/Query/Dto/GridViewModel — тогда генератор
/// регистрирует маршруты (как у абстрактных эндпоинтов товара в Catalog).
/// </summary>
public abstract class CreateDocumentEndpoint<TRequest, TCommand> : CreateCommandEndpoint<TRequest, TCommand>
    where TRequest : CreateDocumentRequestBase
    where TCommand : ICommand<Guid>
{
    public override string Route => SalesDocumentsConstants.DefaultRoutePrefix;
    public override string GetByIdRouteName => "GetSalesDocumentById";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments");
}

public abstract class GetDocumentByIdEndpoint<TRequest, TQuery, TDto>
    : QueryOrNotFoundEndpoint<TRequest, TQuery, TDto, TDto>
    where TRequest : GetDocumentByIdRequestBase
    where TQuery : IQuery<TDto?>
    where TDto : SalesDocumentDtoBase
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetSalesDocumentById").WithTags("SalesDocuments");
}

public abstract class UpdateDocumentEndpoint<TRequest, TCommand> : UpdateCommandEndpoint<TRequest, TCommand>
    where TRequest : UpdateDocumentRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{SalesDocumentsConstants.DefaultRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments");
}

public abstract class GetDocumentsGridEndpoint<TRequest, TQuery, TGridViewModel>
    : QueryGridEndpoint<TRequest, TQuery, TGridViewModel, TGridViewModel>
    where TRequest : GetDocumentsGridRequestBase
    where TQuery : IQuery<GridResult<TGridViewModel>>
    where TGridViewModel : SalesDocumentGridViewModelBase
{
    public override string Route => SalesDocumentsConstants.DefaultRoutePrefix;

    protected override void Configure(EndpointConfiguration config) => config.WithTags("SalesDocuments");
}
