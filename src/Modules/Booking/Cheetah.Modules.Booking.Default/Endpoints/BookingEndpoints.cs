using Cheetah.Modules.Booking.Api.Endpoints;
using Cheetah.Modules.Booking.Default.Contracts;

namespace Cheetah.Modules.Booking.Default.Endpoints;

/// <summary>Конкретные эндпоинты «из коробки» поверх абстрактной базы шаблона.</summary>
public sealed class BookingEndpoints
    : BookingEndpointsBase<CreateBookingTypeRequest, UpdateBookingTypeRequest, BookingTypeDto, BookingDto>;
