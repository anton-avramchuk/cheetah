using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Customer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация клиента: ключ, аудит-игнор доменных событий,
/// общие колонки и VO-конвертеры (<see cref="Email"/>/<see cref="Phone"/> ↔ строка).
/// Наследник наследует её и добавляет свои поля через <see cref="ConfigureCustom"/>.
/// </summary>
public abstract class CustomerConfigurationBase<TCustomer> : IEntityTypeConfiguration<TCustomer>
    where TCustomer : CustomerBase
{
    protected virtual string TableName => CustomerConstants.DefaultTableName;
    protected virtual string Schema => CustomerConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TCustomer> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(CustomerConstants.MaxNameLength)
            .IsRequired();

        // VO-конвертеры: для null конвертер не вызывается (nullable-колонки работают как есть).
        builder.Property(x => x.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasMaxLength(CustomerConstants.MaxEmailLength);

        builder.Property(x => x.Phone)
            .HasConversion(p => p.Value, v => Phone.Create(v))
            .HasMaxLength(CustomerConstants.MaxPhoneLength);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.OwnerId);

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TCustomer> builder)
    {
    }
}
