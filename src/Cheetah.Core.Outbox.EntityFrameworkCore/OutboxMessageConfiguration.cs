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

        // Базовый индекс, работающий на любом провайдере. Для Postgres-нагрузки замените его
        // partial+covered индексом из Cheetah.Core.Outbox.PostgreSql (см. OutboxOptimizedIndexSql.Create()).
        builder.HasIndex(x => new { x.ProcessedAt, x.OccurredAt })
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

public class DeadLetterMessageConfiguration : IEntityTypeConfiguration<DeadLetterMessage>
{
    public void Configure(EntityTypeBuilder<DeadLetterMessage> builder)
    {
        builder.ToTable("DeadLetterMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.MovedToDeadLetterAt).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(4000);

        builder.HasIndex(x => x.MovedToDeadLetterAt);
    }
}
