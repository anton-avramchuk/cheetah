using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Booking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация брони: ключ, игнор доменных событий, колонки, связь с ответами
/// и индексы под анти-дабл-букинг. Уникальный частичный индекс <c>(HostUserId, StartUtc)</c> среди
/// активных статусов (Confirmed/Rescheduled) — БД-уровень защиты от двойной брони слота.
/// Наследник расширяет схему через <see cref="ConfigureCustom"/>.
/// </summary>
public abstract class BookingConfigurationBase<TBooking> : IEntityTypeConfiguration<TBooking>
    where TBooking : BookingBase
{
    protected virtual string TableName => BookingConstants.BookingsTableName;
    protected virtual string Schema => BookingConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TBooking> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InviteeName).HasMaxLength(BookingConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.InviteeEmail).HasMaxLength(BookingConstants.MaxEmailLength).IsRequired();
        builder.Property(x => x.InviteeTimeZone).HasMaxLength(BookingConstants.MaxTimeZoneLength).IsRequired();
        builder.Property(x => x.ManageToken).HasMaxLength(BookingConstants.MaxManageTokenLength).IsRequired();
        builder.Property(x => x.CancelReason).HasMaxLength(BookingConstants.MaxReasonLength);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Attributes).HasColumnType("jsonb");

        builder.OwnsMany(x => x.Answers, nb =>
        {
            nb.ToTable("BookingAnswers", Schema);
            nb.WithOwner().HasForeignKey(a => a.BookingId);
            nb.HasKey(a => a.Id);
            nb.Property(a => a.Question).HasMaxLength(BookingConstants.MaxNameLength).IsRequired();
        });
        builder.Navigation(x => x.Answers)
            .HasField("_answers")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.ManageToken).IsUnique();
        builder.HasIndex(x => new { x.HostUserId, x.Status, x.StartUtc });

        // Анти-дабл-букинг на уровне БД: один активный слот на host'а.
        builder.HasIndex(x => new { x.HostUserId, x.StartUtc })
            .IsUnique()
            .HasFilter("\"Status\" IN (0, 1)"); // Confirmed, Rescheduled

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TBooking> builder)
    {
    }
}
