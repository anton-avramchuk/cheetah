using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Catalog.Application.Categories;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Api.Endpoints.Categories;

public sealed class CreateCategoryEndpoint : CreateCommandEndpoint<CreateCategoryRequest, CreateCategoryCommand>
{
    public override string Route => CatalogConstants.DefaultCategoriesRoutePrefix;
    public override string GetByIdRouteName => "GetCategoryById";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Categories");
}

public sealed class GetCategoryByIdEndpoint
    : QueryOrNotFoundEndpoint<GetCategoryByIdRequest, GetCategoryByIdQuery, ProductCategoryDto, ProductCategoryDto>
{
    public override string Route => $"{CatalogConstants.DefaultCategoriesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetCategoryById").WithTags("Catalog.Categories");
}

public sealed class UpdateCategoryEndpoint : UpdateCommandEndpoint<UpdateCategoryRequest, UpdateCategoryCommand>
{
    public override string Route => $"{CatalogConstants.DefaultCategoriesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Categories");
}

public sealed class DeleteCategoryEndpoint : DeleteCommandEndpoint<DeleteCategoryRequest, DeleteCategoryCommand>
{
    public override string Route => $"{CatalogConstants.DefaultCategoriesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Categories");
}

public sealed class GetCategoriesGridEndpoint
    : QueryGridEndpoint<GetCategoriesGridRequest, GetCategoriesGridQuery, ProductCategoryDto, ProductCategoryDto>
{
    public override string Route => CatalogConstants.DefaultCategoriesRoutePrefix;

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.Categories");
}
