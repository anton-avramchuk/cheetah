using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Exceptions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;

namespace Cheetah.Modules.Booking.Application.BookingTypes;

// ── Создание типа встречи ──────────────────────────────────────────────────────────────────

/// <summary>Создать тип встречи (публичную страницу записи) из запроса наследника.</summary>
public sealed record CreateBookingTypeCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateBookingTypeRequestBase;

public class CreateBookingTypeCommandHandler<TBookingType, TCreateRequest>
    : ICommandHandler<CreateBookingTypeCommand<TCreateRequest>, Guid>
    where TBookingType : BookingTypeBase
    where TCreateRequest : CreateBookingTypeRequestBase
{
    private readonly IBookingTypeFactory<TBookingType, TCreateRequest> _factory;
    private readonly IRepository<TBookingType, Guid> _repository;

    public CreateBookingTypeCommandHandler(
        IBookingTypeFactory<TBookingType, TCreateRequest> factory, IRepository<TBookingType, Guid> repository)
    {
        _factory = factory;
        _repository = repository;
    }

    public async ValueTask<Guid> HandleAsync(CreateBookingTypeCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var type = _factory.Create(command.Request);
        _repository.Add(type);
        await _repository.SaveChangesAsync(ct);
        return type.Id;
    }
}

// ── Обновление типа встречи ────────────────────────────────────────────────────────────────

/// <summary>Обновить базовые поля типа встречи.</summary>
public sealed record UpdateBookingTypeCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateBookingTypeRequestBase;

public class UpdateBookingTypeCommandHandler<TBookingType, TUpdateRequest>
    : ICommandHandler<UpdateBookingTypeCommand<TUpdateRequest>>
    where TBookingType : BookingTypeBase
    where TUpdateRequest : UpdateBookingTypeRequestBase
{
    private readonly IRepository<TBookingType, Guid> _repository;

    public UpdateBookingTypeCommandHandler(IRepository<TBookingType, Guid> repository) => _repository = repository;

    public async ValueTask HandleAsync(UpdateBookingTypeCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var type = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new BookingValidationException($"Booking type '{command.Id}' not found");

        var r = command.Request;
        type.Update(r.Name, r.DurationMinutes, r.LocationKind, r.LocationDetails,
            r.BufferBeforeMinutes, r.BufferAfterMinutes, r.MinNoticeMinutes, r.MaxAdvanceDays,
            r.SlotStepMinutes, r.Color);
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Деактивация типа встречи ───────────────────────────────────────────────────────────────

/// <summary>Деактивировать тип встречи (скрыть страницу записи).</summary>
public sealed record DeactivateBookingTypeCommand(Guid Id) : ICommand;

public class DeactivateBookingTypeCommandHandler<TBookingType> : ICommandHandler<DeactivateBookingTypeCommand>
    where TBookingType : BookingTypeBase
{
    private readonly IRepository<TBookingType, Guid> _repository;

    public DeactivateBookingTypeCommandHandler(IRepository<TBookingType, Guid> repository) => _repository = repository;

    public async ValueTask HandleAsync(DeactivateBookingTypeCommand command, CancellationToken ct = default)
    {
        var type = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new BookingValidationException($"Booking type '{command.Id}' not found");

        type.Deactivate();
        await _repository.SaveChangesAsync(ct);
    }
}
