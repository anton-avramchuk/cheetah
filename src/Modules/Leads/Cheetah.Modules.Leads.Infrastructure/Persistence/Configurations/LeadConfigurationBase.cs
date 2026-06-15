using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Leads.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация лида: ключ, игнор доменных событий, общие колонки,
/// VO-конвертеры (<see cref="Email"/>/<see cref="Phone"/> ↔ строка) и индексы. Наследник наследует её
/// и добавляет свои поля/индексы через <see cref="ConfigureCustom"/> — точка расширения схемы.
/// </summary>
public abstract class LeadConfigurationBase<TLead> : IEntityTypeConfiguration<TLead>
    where TLead : LeadBase
{
    protected virtual string TableName => LeadsConstants.DefaultTableName;
    protected virtual string Schema => LeadsConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TLead> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(LeadsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Company).HasMaxLength(LeadsConstants.MaxNameLength);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Source).HasConversion<int>();
        builder.Property(x => x.DisqualifyReason).HasMaxLength(LeadsConstants.MaxReasonLength);
        builder.Property(x => x.Attributes).HasColumnType("jsonb");

        // VO → строковые колонки: для null конвертер не вызывается (nullable-колонки работают как есть).
        builder.Property(x => x.Email)
            .HasConversion(e => e!.Value, v => Email.Create(v))
            .HasMaxLength(LeadsConstants.MaxEmailLength);
        builder.Property(x => x.Phone)
            .HasConversion(p => p!.Value, v => Phone.Create(v))
            .HasMaxLength(LeadsConstants.MaxPhoneLength);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => new { x.OwnerId, x.Status });

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TLead> builder)
    {
    }
}
