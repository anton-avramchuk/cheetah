using System.Text.Json;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cheetah.Modules.CustomFields.Infrastructure.Persistence.Configurations;

public sealed class CustomFieldEntityTypeConfiguration : IEntityTypeConfiguration<CustomFieldEntityType>
{
    public void Configure(EntityTypeBuilder<CustomFieldEntityType> b)
    {
        b.ToTable("CustomFieldEntityTypes", CustomFieldsConstants.Schema);
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.Key).HasMaxLength(CustomFieldsConstants.MaxEntityTypeLength).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(CustomFieldsConstants.MaxLabelLength).IsRequired();
        b.Property(x => x.OwnerService).HasMaxLength(CustomFieldsConstants.MaxOwnerServiceLength).IsRequired();
        b.Property(x => x.IdType).HasConversion<int>();

        b.HasIndex(x => x.Key).IsUnique();
        b.HasIndex(x => x.OwnerService);
    }
}

public sealed class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> b)
    {
        b.ToTable("CustomFieldDefinitions", CustomFieldsConstants.Schema);
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.EntityType).HasMaxLength(CustomFieldsConstants.MaxEntityTypeLength).IsRequired();
        b.Property(x => x.Key).HasMaxLength(CustomFieldsConstants.MaxKeyLength).IsRequired();
        b.Property(x => x.Label).HasMaxLength(CustomFieldsConstants.MaxLabelLength).IsRequired();
        b.Property(x => x.DataType).HasConversion<int>();
        b.Property(x => x.ValidationRulesJson).HasColumnType("jsonb");
        b.Property(x => x.VisibilityRule).HasColumnType("jsonb");

        var stringListConverter = new ValueConverter<IReadOnlyList<string>?, string?>(
            v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null));

        var stringListComparer = new ValueComparer<IReadOnlyList<string>?>(
            (a, c) => (a == null && c == null) || (a != null && c != null && a.SequenceEqual(c)),
            v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v == null ? null : v.ToList());

        b.Property(x => x.Options)
            .HasColumnType("jsonb")
            .HasConversion(stringListConverter, stringListComparer);

        // Уникальность ключа в скоупе (TenantId, EntityType); TenantId NULL для глобальных шаблонов.
        b.HasIndex(x => new { x.TenantId, x.EntityType, x.Key }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.EntityType });
    }
}

public sealed class CustomFieldValueSetConfiguration : IEntityTypeConfiguration<CustomFieldValueSet>
{
    public void Configure(EntityTypeBuilder<CustomFieldValueSet> b)
    {
        b.ToTable("CustomFieldValueSets", CustomFieldsConstants.Schema);
        b.HasKey(x => x.Id);
        b.Ignore(x => x.DomainEvents);

        b.Property(x => x.EntityType).HasMaxLength(CustomFieldsConstants.MaxEntityTypeLength).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(CustomFieldsConstants.MaxEntityIdLength).IsRequired();
        b.Property(x => x.ValuesJson).HasColumnType("jsonb").IsRequired();

        // Один набор значений на сущность.
        b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId }).IsUnique();
    }
}
