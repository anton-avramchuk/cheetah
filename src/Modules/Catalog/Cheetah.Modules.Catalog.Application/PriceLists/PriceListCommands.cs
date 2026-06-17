using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Catalog.Application.Exceptions;
using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Domain.Specifications;

namespace Cheetah.Modules.Catalog.Application.PriceLists;

// ── Создание прайс-листа ─────────────────────────────────────────────────────────────────────

/// <summary>Создать прайс-лист.</summary>
public sealed record CreatePriceListCommand(
    string Name, string Currency, bool IsDefault, DateTimeOffset? ValidFrom, DateTimeOffset? ValidTo)
    : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreatePriceListCommand, Guid>))]
public sealed class CreatePriceListCommandHandler : ICommandHandler<CreatePriceListCommand, Guid>
{
    private readonly IRepository<PriceList, Guid> _repository;

    public CreatePriceListCommandHandler(IRepository<PriceList, Guid> repository)
        => _repository = repository;

    public async ValueTask<Guid> HandleAsync(CreatePriceListCommand command, CancellationToken ct = default)
    {
        var priceList = PriceList.Create(
            command.Name, command.Currency, command.IsDefault, command.ValidFrom, command.ValidTo);
        _repository.Add(priceList);
        await _repository.SaveChangesAsync(ct);
        return priceList.Id;
    }
}

// ── Обновление прайс-листа ───────────────────────────────────────────────────────────────────

/// <summary>Обновить заголовок прайс-листа (имя, флаг по умолчанию, период действия).</summary>
public sealed record UpdatePriceListCommand(
    Guid Id, string Name, bool IsDefault, DateTimeOffset? ValidFrom, DateTimeOffset? ValidTo) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdatePriceListCommand>))]
public sealed class UpdatePriceListCommandHandler : ICommandHandler<UpdatePriceListCommand>
{
    private readonly IRepository<PriceList, Guid> _repository;

    public UpdatePriceListCommandHandler(IRepository<PriceList, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(UpdatePriceListCommand command, CancellationToken ct = default)
    {
        var priceList = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Price list '{command.Id}' not found");

        priceList.Rename(command.Name);
        priceList.SetDefault(command.IsDefault);
        priceList.ChangeValidity(command.ValidFrom, command.ValidTo);
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Удаление прайс-листа ─────────────────────────────────────────────────────────────────────

/// <summary>Удалить прайс-лист (вместе со строками).</summary>
public sealed record DeletePriceListCommand(Guid Id) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeletePriceListCommand>))]
public sealed class DeletePriceListCommandHandler : ICommandHandler<DeletePriceListCommand>
{
    private readonly IRepository<PriceList, Guid> _repository;

    public DeletePriceListCommandHandler(IRepository<PriceList, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(DeletePriceListCommand command, CancellationToken ct = default)
    {
        var priceList = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new CatalogValidationException($"Price list '{command.Id}' not found");

        _repository.Delete(priceList);
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Установка цены ───────────────────────────────────────────────────────────────────────────

/// <summary>Установить/изменить цену товара в прайс-листе.</summary>
public sealed record SetPriceCommand(Guid PriceListId, Guid ProductId, decimal Price, int? MinQty) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SetPriceCommand>))]
public sealed class SetPriceCommandHandler : ICommandHandler<SetPriceCommand>
{
    private readonly IRepository<PriceList, Guid> _repository;
    private readonly IEventBus _eventBus;

    public SetPriceCommandHandler(IRepository<PriceList, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(SetPriceCommand command, CancellationToken ct = default)
    {
        var priceList = await _repository.GetBySpecAsync(new PriceListByIdSpecification(command.PriceListId), ct)
            ?? throw new CatalogValidationException($"Price list '{command.PriceListId}' not found");

        priceList.SetPrice(command.ProductId, command.Price, command.MinQty);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in priceList.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        priceList.ClearDomainEvents();
    }
}
