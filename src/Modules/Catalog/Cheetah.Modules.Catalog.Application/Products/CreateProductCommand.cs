using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Catalog.Application.Abstractions;
using Cheetah.Modules.Catalog.Application.Exceptions;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Domain.Specifications;

namespace Cheetah.Modules.Catalog.Application.Products;

/// <summary>Создать товар из запроса наследника.</summary>
public sealed record CreateProductCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateProductRequestBase;

public class CreateProductCommandHandler<TProduct, TCreateRequest>
    : ICommandHandler<CreateProductCommand<TCreateRequest>, Guid>
    where TProduct : ProductBase
    where TCreateRequest : CreateProductRequestBase
{
    private readonly IProductFactory<TProduct, TCreateRequest> _factory;
    private readonly IRepository<TProduct, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateProductCommandHandler(
        IProductFactory<TProduct, TCreateRequest> factory,
        IRepository<TProduct, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateProductCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var sku = command.Request.Sku;
        if (await _repository.ExistsAsync(new ProductBySkuSpecification<TProduct>(sku), ct))
            throw new CatalogValidationException($"Product with SKU '{sku}' already exists");

        var product = _factory.Create(command.Request);
        _repository.Add(product);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in product.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        product.ClearDomainEvents();

        return product.Id;
    }
}
