using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Catalog.Application.Exceptions;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Application.Products;

// ── Обновление базовых полей ─────────────────────────────────────────────────────────────────

/// <summary>Обновить базовые поля товара (имя, описание, категория).</summary>
public sealed record UpdateProductCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateProductRequestBase;

public class UpdateProductCommandHandler<TProduct, TUpdateRequest>
    : ICommandHandler<UpdateProductCommand<TUpdateRequest>>
    where TProduct : ProductBase
    where TUpdateRequest : UpdateProductRequestBase
{
    private readonly IRepository<TProduct, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateProductCommandHandler(IRepository<TProduct, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateProductCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Product '{command.Id}' not found");

        product.Update(command.Request.Name, command.Request.Description, command.Request.CategoryId);
        await ProductHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, product, ct);
    }
}

// ── Деактивация ──────────────────────────────────────────────────────────────────────────────

/// <summary>Деактивировать товар (снять с продажи).</summary>
public sealed record DeactivateProductCommand(Guid Id) : ICommand;

public class DeactivateProductCommandHandler<TProduct> : ICommandHandler<DeactivateProductCommand>
    where TProduct : ProductBase
{
    private readonly IRepository<TProduct, Guid> _repository;
    private readonly IEventBus _eventBus;

    public DeactivateProductCommandHandler(IRepository<TProduct, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(DeactivateProductCommand command, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Product '{command.Id}' not found");

        product.Deactivate();
        await ProductHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, product, ct);
    }
}

// ── Активация ────────────────────────────────────────────────────────────────────────────────

/// <summary>Активировать ранее деактивированный товар.</summary>
public sealed record ActivateProductCommand(Guid Id) : ICommand;

public class ActivateProductCommandHandler<TProduct> : ICommandHandler<ActivateProductCommand>
    where TProduct : ProductBase
{
    private readonly IRepository<TProduct, Guid> _repository;
    private readonly IEventBus _eventBus;

    public ActivateProductCommandHandler(IRepository<TProduct, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ActivateProductCommand command, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Product '{command.Id}' not found");

        product.Activate();
        await ProductHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, product, ct);
    }
}

// ── Удаление ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Удалить товар физически (жёсткое удаление; мягкое — через Deactivate).</summary>
public sealed record DeleteProductCommand(Guid Id) : ICommand;

public class DeleteProductCommandHandler<TProduct> : ICommandHandler<DeleteProductCommand>
    where TProduct : ProductBase
{
    private readonly IRepository<TProduct, Guid> _repository;

    public DeleteProductCommandHandler(IRepository<TProduct, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(DeleteProductCommand command, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Product '{command.Id}' not found");

        _repository.Delete(product);
        await _repository.SaveChangesAsync(ct);
    }
}
