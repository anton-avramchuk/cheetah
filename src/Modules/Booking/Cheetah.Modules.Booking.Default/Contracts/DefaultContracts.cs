using Cheetah.Modules.Booking.Contracts;

namespace Cheetah.Modules.Booking.Default.Contracts;

/// <summary>Конкретные Contracts «из коробки» (без доп. полей). Наследник модуля объявляет свои.</summary>
public sealed record CreateBookingTypeRequest : CreateBookingTypeRequestBase;

public sealed record UpdateBookingTypeRequest : UpdateBookingTypeRequestBase;

public sealed record BookingTypeDto : BookingTypeDtoBase;

public sealed record BookingDto : BookingDtoBase;
