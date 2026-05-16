using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.Error).HasMaxLength(4000);

        // Горячий индекс: процессор ищет ProcessedAt IS NULL, отсортированные по OccurredAt.
        builder.HasIndex(x => new { x.ProcessedAt, x.NextAttemptAt, x.OccurredAt })
            .HasDatabaseName("IX_OutboxMessages_Pending");
    }
}

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => new { x.EventId, x.ConsumerName });

        builder.Property(x => x.ConsumerName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ReceivedAt).IsRequired();
    }
}
