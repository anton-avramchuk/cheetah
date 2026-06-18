using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Specifications;

namespace Cheetah.Modules.Booking.Application.BookingTypes;

// ── Публичная страница записи по slug ──────────────────────────────────────────────────────

/// <summary>Публичное описание страницы записи по slug (null, если нет/неактивна).</summary>
public sealed record GetPublicPageQuery(string Slug) : IQuery<PublicBookingPageDto?>;

public class GetPublicPageQueryHandler<TBookingType> : IQueryHandler<GetPublicPageQuery, PublicBookingPageDto?>
    where TBookingType : BookingTypeBase
{
    private readonly IRepository<TBookingType, Guid> _repository;

    public GetPublicPageQueryHandler(IRepository<TBookingType, Guid> repository) => _repository = repository;

    public async ValueTask<PublicBookingPageDto?> HandleAsync(GetPublicPageQuery query, CancellationToken ct = default)
    {
        var type = await _repository.GetBySpecAsync(new BookingTypeBySlugSpecification<TBookingType>(query.Slug), ct);
        return type is null
            ? null
            : new PublicBookingPageDto(type.Slug, type.Name, type.DurationMinutes, type.LocationKind, type.LocationDetails);
    }
}

// ── Тип встречи по Id (host) ───────────────────────────────────────────────────────────────

/// <summary>Получить тип встречи по идентификатору (null, если не найден).</summary>
public sealed record GetBookingTypeByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : BookingTypeDtoBase;

public class GetBookingTypeByIdQueryHandler<TBookingType, TDto> : IQueryHandler<GetBookingTypeByIdQuery<TDto>, TDto?>
    where TBookingType : BookingTypeBase
    where TDto : BookingTypeDtoBase
{
    private readonly IRepository<TBookingType, Guid> _repository;
    private readonly IBookingTypeProjector<TBookingType, TDto> _projector;

    public GetBookingTypeByIdQueryHandler(
        IRepository<TBookingType, Guid> repository, IBookingTypeProjector<TBookingType, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetBookingTypeByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var type = await _repository.GetByIdAsync(query.Id, ct);
        return type is null ? null : _projector.ToDto(type);
    }
}
