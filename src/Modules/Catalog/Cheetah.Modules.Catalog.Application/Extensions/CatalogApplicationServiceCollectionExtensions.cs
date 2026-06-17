using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Catalog.Application.Abstractions;
using Cheetah.Modules.Catalog.Application.Products;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Catalog.Application.Extensions;

public static class CatalogApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы товара конкретной реализации
    /// (включая удаление и грид). Хендлеры категорий и прайс-листов не зависят от типа товара и
    /// регистрируются генератором по <c>[Export]</c>. Вызывается из прикладного модуля наследника
    /// после <c>AddCatalogInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddCatalogApplication<
        TProduct, TCreateRequest, TUpdateRequest, TDto, TGridViewModel, TFactory, TProjector>(
        this IServiceCollection services)
        where TProduct : ProductBase
        where TCreateRequest : CreateProductRequestBase
        where TUpdateRequest : UpdateProductRequestBase
        where TDto : ProductDtoBase
        where TGridViewModel : ProductGridViewModelBase
        where TFactory : class, IProductFactory<TProduct, TCreateRequest>
        where TProjector : class, IProductProjector<TProduct, TDto>
    {
        services.AddScoped<IProductFactory<TProduct, TCreateRequest>, TFactory>();
        services.AddScoped<IProductProjector<TProduct, TDto>, TProjector>();

        services.AddScoped<ICommandHandler<CreateProductCommand<TCreateRequest>, Guid>,
            CreateProductCommandHandler<TProduct, TCreateRequest>>();
        services.AddScoped<ICommandHandler<UpdateProductCommand<TUpdateRequest>>,
            UpdateProductCommandHandler<TProduct, TUpdateRequest>>();
        services.AddScoped<ICommandHandler<DeactivateProductCommand>,
            DeactivateProductCommandHandler<TProduct>>();
        services.AddScoped<ICommandHandler<ActivateProductCommand>,
            ActivateProductCommandHandler<TProduct>>();
        services.AddScoped<ICommandHandler<DeleteProductCommand>,
            DeleteProductCommandHandler<TProduct>>();

        services.AddScoped<IQueryHandler<GetProductByIdQuery<TDto>, TDto?>,
            GetProductByIdQueryHandler<TProduct, TDto>>();
        services.AddScoped<IQueryHandler<ListProductsQuery<TDto>, IReadOnlyList<TDto>>,
            ListProductsQueryHandler<TProduct, TDto>>();
        services.AddScoped<IQueryHandler<GetProductsGridQuery<TGridViewModel>, GridResult<TGridViewModel>>,
            GetProductsGridQueryHandler<TProduct, TGridViewModel>>();

        return services;
    }
}
