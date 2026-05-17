using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Audit.EntityFrameworkCore;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(512);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Action).IsRequired();
        builder.Property(x => x.Changes).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();

        builder.Property(x => x.UserId).HasMaxLength(256);
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.Property(x => x.TenantId).HasMaxLength(128);
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.Error).HasMaxLength(4000);

        // Горячий индекс для KafkaAuditPublisher.
        builder.HasIndex(x => new { x.PublishedAt, x.OccurredAt })
            .HasDatabaseName("IX_AuditEntries_Unpublished");

        // Для запросов по сущности (UI истории изменений).
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt })
            .HasDatabaseName("IX_AuditEntries_Entity");
    }
}
