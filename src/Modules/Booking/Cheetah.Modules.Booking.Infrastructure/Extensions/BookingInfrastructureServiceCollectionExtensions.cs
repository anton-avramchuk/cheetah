using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Infrastructure.Calendar;
using Cheetah.Modules.Booking.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Modules.Booking.Infrastructure.Extensions;

public static class BookingInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктуру конкретной реализации Booking: DbContext (типы встреч, расписания,
    /// брони), мигратор, провайдер PostgreSQL, EF-репозитории трёх агрегатов и шлюз к Calendar
    /// (<see cref="IBookingCalendarGateway"/>). Резолвер календаря host'а по умолчанию — no-op
    /// (<see cref="NullHostCalendarResolver"/>); приложение может зарегистрировать свой до вызова.
    /// </summary>
    public static IServiceCollection AddBookingInfrastructure<TContext, TBooking, TBookingType, TSchedule>(
        this IServiceCollection services)
        where TContext : BookingDbContextBase<TContext, TBooking, TBookingType, TSchedule>
        where TBooking : BookingBase
        where TBookingType : BookingTypeBase
        where TSchedule : AvailabilityScheduleBase
    {
        services.AddApplicationDbContext<TContext>();
        services.AddScoped<TContext>();
        services.AddDatabaseMigrator<TContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TContext>(); });

        services.AddScoped<IRepository<TBooking, Guid>, EfRepository<TContext, TBooking, Guid>>();
        services.AddScoped<IRepository<TBookingType, Guid>, EfRepository<TContext, TBookingType, Guid>>();
        services.AddScoped<IRepository<TSchedule, Guid>, EfRepository<TContext, TSchedule, Guid>>();

        services.TryAddScoped<IHostCalendarResolver, NullHostCalendarResolver>();
        services.AddScoped<IBookingCalendarGateway, BookingCalendarGateway>();

        return services;
    }
}
