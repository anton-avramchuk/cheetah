using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Api.Endpoints.Products;

/// <summary>
/// Абстрактные шаблоны эндпоинтов товара (товар расширяем). Наследник/хост закрывает generic-параметры
/// своими конкретными Request/Command/Query/Dto/GridViewModel — тогда генератор регистрирует маршруты
/// (как у абстрактных эндпоинтов Identity). Маршруты и имена можно переопределить.
/// </summary>
public abstract class CreateProductEndpoint<TRequest, TCommand> : CreateCommandEndpoint<TRequest, TCommand>
    where TRequest : CreateProductRequestBase
    where TCommand : ICommand<Guid>
{
    public override string Route => CatalogConstants.DefaultProductsRoutePrefix;
    public override string GetByIdRouteName => "GetProductById";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Products");
}

public abstract class GetProductByIdEndpoint<TRequest, TQuery, TDto>
    : QueryOrNotFoundEndpoint<TRequest, TQuery, TDto, TDto>
    where TRequest : GetProductByIdRequestBase
    where TQuery : IQuery<TDto?>
    where TDto : ProductDtoBase
{
    public override string Route => $"{CatalogConstants.DefaultProductsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetProductById").WithTags("Catalog.Products");
}

public abstract class UpdateProductEndpoint<TRequest, TCommand> : UpdateCommandEndpoint<TRequest, TCommand>
    where TRequest : UpdateProductRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{CatalogConstants.DefaultProductsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Products");
}

public abstract class DeleteProductEndpoint<TRequest, TCommand> : DeleteCommandEndpoint<TRequest, TCommand>
    where TRequest : DeleteProductRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{CatalogConstants.DefaultProductsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Products");
}

public abstract class GetProductsGridEndpoint<TRequest, TQuery, TGridViewModel>
    : QueryGridEndpoint<TRequest, TQuery, TGridViewModel, TGridViewModel>
    where TRequest : GetProductsGridRequestBase
    where TQuery : IQuery<GridResult<TGridViewModel>>
    where TGridViewModel : ProductGridViewModelBase
{
    public override string Route => CatalogConstants.DefaultProductsRoutePrefix;

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Products");
}
