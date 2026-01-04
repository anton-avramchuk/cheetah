using Cheetah.Core.Tenants.Domain;
using Cheetah.Core.Tenants.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Core.EntityFramework.Tenants.Configurations;

/// <summary>
/// Abstract base configuration for TenantEntity
/// </summary>
public abstract class TenantEntityConfiguration<TTenant, TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
    : IEntityTypeConfiguration<TTenant>
    where TTenant : TenantEntity<TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
    where TTenantCreatedEvent : TenantCreatedEvent
    where TTenantUpdatedEvent : TenantUpdatedEvent
    where TTenantDeactivatedEvent : TenantDeactivatedEvent
    where TTenantActivatedEvent : TenantActivatedEvent
{
    protected virtual TenantEntityConfigurationOptions Options { get; } = new();

    public virtual void Configure(EntityTypeBuilder<TTenant> builder)
    {
        // Table name
        builder.ToTable(Options.TableName);

        // Primary key
        builder.HasKey(t => t.Id);

        // Name - required, unique
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(Options.NameMaxLength);

        if (Options.CreateNameUniqueIndex)
        {
            builder.HasIndex(t => t.Name)
                .IsUnique()
                .HasDatabaseName(Options.NameIndexName);
        }

        // Description - optional
        builder.Property(t => t.Description)
            .HasMaxLength(Options.DescriptionMaxLength);

        // IsActive - required
        builder.Property(t => t.IsActive)
            .IsRequired();

        if (Options.CreateIsActiveIndex)
        {
            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName(Options.IsActiveIndexName);
        }

        // CreatedAt
        builder.Property(t => t.CreatedAt)
            .IsRequired(false);

        if (Options.CreateCreatedAtIndex)
        {
            builder.HasIndex(t => t.CreatedAt)
                .HasDatabaseName(Options.CreatedAtIndexName);
        }

        // UpdatedAt
        builder.Property(t => t.UpdatedAt)
            .IsRequired(false);

        // RemovedAt
        builder.Property(t => t.RemovedAt)
            .IsRequired(false);

        // Ignore domain events collection
        builder.Ignore(t => t.DomainEvents);

        // Allow derived classes to configure additional properties
        ConfigureAdditionalProperties(builder);
    }

    /// <summary>
    /// Override this method to configure additional properties specific to derived tenant entities
    /// </summary>
    protected virtual void ConfigureAdditionalProperties(EntityTypeBuilder<TTenant> builder)
    {
        // Override in derived classes
    }
}
