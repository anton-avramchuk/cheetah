using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Catalog.Application.PriceLists;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Api.Endpoints.PriceLists;

public sealed class CreatePriceListEndpoint : CreateCommandEndpoint<CreatePriceListRequest, CreatePriceListCommand>
{
    public override string Route => CatalogConstants.DefaultPriceListsRoutePrefix;
    public override string GetByIdRouteName => "GetPriceListById";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.PriceLists");
}

public sealed class GetPriceListByIdEndpoint
    : QueryOrNotFoundEndpoint<GetPriceListByIdRequest, GetPriceListByIdQuery, PriceListDto, PriceListDto>
{
    public override string Route => $"{CatalogConstants.DefaultPriceListsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetPriceListById").WithTags("Catalog.PriceLists");
}

public sealed class UpdatePriceListEndpoint : UpdateCommandEndpoint<UpdatePriceListRequest, UpdatePriceListCommand>
{
    public override string Route => $"{CatalogConstants.DefaultPriceListsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.PriceLists");
}

public sealed class DeletePriceListEndpoint : DeleteCommandEndpoint<DeletePriceListRequest, DeletePriceListCommand>
{
    public override string Route => $"{CatalogConstants.DefaultPriceListsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.PriceLists");
}

public sealed class GetPriceListsGridEndpoint
    : QueryGridEndpoint<GetPriceListsGridRequest, GetPriceListsGridQuery, PriceListGridViewModel, PriceListGridViewModel>
{
    public override string Route => CatalogConstants.DefaultPriceListsRoutePrefix;

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.PriceLists");
}

public sealed class SetPriceEndpoint : CommandEndpoint<SetPriceRequest, SetPriceCommand>
{
    public override string Route => $"{CatalogConstants.DefaultPriceListsRoutePrefix}/{{id:guid}}/items";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Catalog.PriceLists");
}

public sealed class ResolvePriceEndpoint
    : QueryOrNotFoundEndpoint<ResolvePriceRequest, ResolvePriceQuery, ResolvedPriceDto, ResolvedPriceDto>
{
    public override string Route => $"{CatalogConstants.DefaultPriceListsRoutePrefix}/{{id:guid}}/price";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("ResolvePrice").WithTags("Catalog.PriceLists");
}
