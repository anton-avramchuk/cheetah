using Cheetah.Core.CQRS;
using Cheetah.Modules.Booking.Application.Bookings;
using Cheetah.Modules.Booking.Application.BookingTypes;
using Cheetah.Modules.Booking.Application.Availability;
using Cheetah.Modules.Booking.Application.Exceptions;
using Cheetah.Modules.Booking.Application.Slots;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Booking.Api.Endpoints;

/// <summary>
/// Абстрактная база эндпоинтов Booking (Customer-стиль: <see cref="IDispatcher"/> + виртуальные методы).
/// Generic над конкретными Contracts наследника. Публичные маршруты записи анонимны (помечены
/// <c>AllowAnonymous</c>; политику RateLimit применяет приложение); приватные — под авторизацией host'а.
/// Любой маршрут можно переопределить.
/// </summary>
public abstract class BookingEndpointsBase<TCreateTypeRequest, TUpdateTypeRequest, TBookingTypeDto, TBookingDto>
    where TCreateTypeRequest : CreateBookingTypeRequestBase
    where TUpdateTypeRequest : UpdateBookingTypeRequestBase
    where TBookingTypeDto : BookingTypeDtoBase
    where TBookingDto : BookingDtoBase
{
    protected virtual string PublicPrefix => BookingConstants.PublicRoutePrefix;
    protected virtual string BookingTypesPrefix => BookingConstants.BookingTypesRoutePrefix;
    protected virtual string AvailabilityPrefix => BookingConstants.AvailabilityRoutePrefix;
    protected virtual string BookingsPrefix => BookingConstants.BookingsRoutePrefix;

    public void Map(IEndpointRouteBuilder routes)
    {
        var pub = PublicPrefix.TrimEnd('/');
        var types = BookingTypesPrefix.TrimEnd('/');
        var avail = AvailabilityPrefix.TrimEnd('/');
        var bookings = BookingsPrefix.TrimEnd('/');

        // ── Публичные (анонимные) ──
        routes.MapGet($"{pub}/{{slug}}", GetPublicPageAsync).WithName("GetBookingPage").WithTags("Booking").AllowAnonymous();
        routes.MapGet($"{pub}/{{slug}}/slots", GetSlotsAsync).WithName("GetBookingSlots").WithTags("Booking").AllowAnonymous();
        routes.MapPost($"{pub}/{{slug}}", CreateBookingAsync).WithName("CreateBooking").WithTags("Booking").AllowAnonymous();
        routes.MapPost($"{pub}/manage/{{token}}/reschedule", RescheduleAsync).WithName("RescheduleBooking").WithTags("Booking").AllowAnonymous();
        routes.MapPost($"{pub}/manage/{{token}}/cancel", CancelAsync).WithName("CancelBooking").WithTags("Booking").AllowAnonymous();

        // ── Приватные (host) ──
        routes.MapPost(types, CreateTypeAsync).WithName("CreateBookingType").WithTags("BookingTypes");
        routes.MapPut($"{types}/{{id:guid}}", UpdateTypeAsync).WithName("UpdateBookingType").WithTags("BookingTypes");
        routes.MapPost($"{types}/{{id:guid}}/deactivate", DeactivateTypeAsync).WithName("DeactivateBookingType").WithTags("BookingTypes");
        routes.MapGet($"{types}/{{id:guid}}", GetTypeByIdAsync).WithName("GetBookingType").WithTags("BookingTypes");
        routes.MapPut(avail, UpsertAvailabilityAsync).WithName("UpsertAvailability").WithTags("Booking");
        routes.MapGet(bookings, ListBookingsAsync).WithName("ListBookings").WithTags("Bookings");
        routes.MapGet($"{bookings}/{{id:guid}}", GetBookingByIdAsync).WithName("GetBooking").WithTags("Bookings");
        routes.MapPost($"{bookings}/{{id:guid}}/no-show", NoShowAsync).WithName("MarkBookingNoShow").WithTags("Bookings");
    }

    // ── Публичные ──

    protected virtual async Task<IResult> GetPublicPageAsync(
        [FromRoute] string slug, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var page = await dispatcher.QueryAsync<GetPublicPageQuery, PublicBookingPageDto?>(new GetPublicPageQuery(slug), ct);
        return page is null ? Results.NotFound() : Results.Ok(page);
    }

    protected virtual async Task<IResult> GetSlotsAsync(
        [FromRoute] string slug, [FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? tz,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var slots = await dispatcher.QueryAsync<GetAvailableSlotsQuery, IReadOnlyList<SlotDto>>(
            new GetAvailableSlotsQuery(slug, from, to, string.IsNullOrWhiteSpace(tz) ? "UTC" : tz), ct);
        return Results.Ok(slots);
    }

    protected virtual Task<IResult> CreateBookingAsync(
        [FromRoute] string slug, [FromBody] CreatePublicBookingRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            var id = await dispatcher.SendAsync<CreateBookingCommand, Guid>(new CreateBookingCommand(slug, request), ct);
            return Results.Created($"/{BookingsPrefix.TrimEnd('/')}/{id}", id);
        });

    protected virtual Task<IResult> RescheduleAsync(
        [FromRoute] string token, [FromBody] RescheduleBookingRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            await dispatcher.SendAsync(new RescheduleBookingCommand(token, request.NewStartUtc), ct);
            return Results.NoContent();
        });

    protected virtual Task<IResult> CancelAsync(
        [FromRoute] string token, [FromBody] CancelBookingRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            await dispatcher.SendAsync(new CancelBookingCommand(token, request.Reason, ByInvitee: true), ct);
            return Results.NoContent();
        });

    // ── Приватные ──

    protected virtual Task<IResult> CreateTypeAsync(
        [FromBody] TCreateTypeRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            var id = await dispatcher.SendAsync<CreateBookingTypeCommand<TCreateTypeRequest>, Guid>(
                new CreateBookingTypeCommand<TCreateTypeRequest>(request), ct);
            return Results.Created($"/{BookingTypesPrefix.TrimEnd('/')}/{id}", id);
        });

    protected virtual Task<IResult> UpdateTypeAsync(
        [FromRoute] Guid id, [FromBody] TUpdateTypeRequest request,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            await dispatcher.SendAsync(new UpdateBookingTypeCommand<TUpdateTypeRequest>(id, request), ct);
            return Results.NoContent();
        });

    protected virtual Task<IResult> DeactivateTypeAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            await dispatcher.SendAsync(new DeactivateBookingTypeCommand(id), ct);
            return Results.NoContent();
        });

    protected virtual async Task<IResult> GetTypeByIdAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetBookingTypeByIdQuery<TBookingTypeDto>, TBookingTypeDto?>(
            new GetBookingTypeByIdQuery<TBookingTypeDto>(id), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual Task<IResult> UpsertAvailabilityAsync(
        [FromBody] UpsertAvailabilityRequest request, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            var id = await dispatcher.SendAsync<UpsertAvailabilityCommand, Guid>(new UpsertAvailabilityCommand(request), ct);
            return Results.Ok(id);
        });

    protected virtual async Task<IResult> ListBookingsAsync(
        [FromQuery] Guid? hostUserId, [FromQuery] BookingStatus? status,
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListBookingsQuery<TBookingDto>, IReadOnlyList<TBookingDto>>(
            new ListBookingsQuery<TBookingDto>(hostUserId, status, from, to), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> GetBookingByIdAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetBookingByIdQuery<TBookingDto>, TBookingDto?>(
            new GetBookingByIdQuery<TBookingDto>(id), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual Task<IResult> NoShowAsync(
        [FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct)
        => GuardedAsync(async () =>
        {
            await dispatcher.SendAsync(new MarkNoShowCommand(id), ct);
            return Results.NoContent();
        });

    /// <summary>Единая трансляция доменных исключений в HTTP-ответы.</summary>
    protected static async Task<IResult> GuardedAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (BookingConflictException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (BookingValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
