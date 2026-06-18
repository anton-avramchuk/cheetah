using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Booking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация типа встречи: ключ, игнор доменных событий, колонки, уникальный
/// slug. Наследник добавляет свои поля/индексы через <see cref="ConfigureCustom"/>.
/// </summary>
public abstract class BookingTypeConfigurationBase<TBookingType> : IEntityTypeConfiguration<TBookingType>
    where TBookingType : BookingTypeBase
{
    protected virtual string TableName => BookingConstants.BookingTypesTableName;
    protected virtual string Schema => BookingConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TBookingType> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(BookingConstants.MaxSlugLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(BookingConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.LocationKind).HasConversion<int>();
        builder.Property(x => x.Attributes).HasColumnType("jsonb");

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.HostUserId);

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TBookingType> builder)
    {
    }
}
