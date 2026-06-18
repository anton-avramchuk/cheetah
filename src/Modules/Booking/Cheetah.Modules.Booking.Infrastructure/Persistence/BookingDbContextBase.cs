using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Booking.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля Booking. Хостит типы встреч, расписания доступности
/// (с детьми) и брони (с ответами) в одной БД. Наследник закрывает его конкретными типами:
/// <c>class AppBookingDbContext : BookingDbContextBase&lt;AppBookingDbContext, Booking, BookingType, AvailabilitySchedule&gt;</c>
/// и поставляет конфигурации агрегатов. Миграции — у наследника. Имя подключения по умолчанию —
/// <see cref="BookingConstants.ConnectionStringName"/>.
/// </summary>
[ConnectionStringName(BookingConstants.ConnectionStringName)]
public abstract class BookingDbContextBase<TContext, TBooking, TBookingType, TSchedule> : CrmDbContext<TContext>
    where TContext : DbContext
    where TBooking : BookingBase
    where TBookingType : BookingTypeBase
    where TSchedule : AvailabilityScheduleBase
{
    public DbSet<TBooking> Bookings => Set<TBooking>();
    public DbSet<TBookingType> BookingTypes => Set<TBookingType>();
    public DbSet<TSchedule> Schedules => Set<TSchedule>();

    protected BookingDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Дети агрегатов (ответы, недельные правила, исключения дат) сконфигурированы как owned-коллекции
        // внутри конфигураций агрегатов — отдельного применения не требуют.
        modelBuilder.ApplyConfiguration(CreateBookingTypeConfiguration());
        modelBuilder.ApplyConfiguration(CreateScheduleConfiguration());
        modelBuilder.ApplyConfiguration(CreateBookingConfiguration());
    }

    protected abstract IEntityTypeConfiguration<TBookingType> CreateBookingTypeConfiguration();
    protected abstract IEntityTypeConfiguration<TSchedule> CreateScheduleConfiguration();
    protected abstract IEntityTypeConfiguration<TBooking> CreateBookingConfiguration();
}
