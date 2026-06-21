using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Customer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация контактного лица: ключ, аудит-игнор доменных событий,
/// общие колонки, VO-конвертеры (<see cref="Email"/>/<see cref="Phone"/> ↔ строка) и индекс по
/// <c>CustomerId</c>. Наследник наследует её и добавляет свои поля через <see cref="ConfigureCustom"/>.
/// </summary>
public abstract class ContactConfigurationBase<TContact> : IEntityTypeConfiguration<TContact>
    where TContact : ContactBase
{
    protected virtual string TableName => CustomerConstants.DefaultContactsTableName;
    protected virtual string Schema => CustomerConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TContact> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasMaxLength(CustomerConstants.MaxFullNameLength)
            .IsRequired();

        builder.Property(x => x.Position)
            .HasMaxLength(CustomerConstants.MaxPositionLength);

        // VO-конвертеры: для null конвертер не вызывается (nullable-колонки работают как есть),
        // поэтому разыменование e!/p! здесь безопасно.
        builder.Property(x => x.Email)
            .HasConversion(e => e!.Value, v => Email.Create(v))
            .HasMaxLength(CustomerConstants.MaxEmailLength);

        builder.Property(x => x.Phone)
            .HasConversion(p => p!.Value, v => Phone.Create(v))
            .HasMaxLength(CustomerConstants.MaxPhoneLength);

        builder.HasIndex(x => x.CustomerId);

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TContact> builder)
    {
    }
}
