using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Core.Inbox.EntityFrameworkCore;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => new { x.EventId, x.ConsumerName });

        builder.Property(x => x.ConsumerName).IsRequired().HasMaxLength(256);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ReceivedAt).IsRequired();

        // Базовый индекс для cleanup — на любом провайдере. Для Postgres-нагрузки замените
        // partial-индексом из Cheetah.Core.Inbox.PostgreSql (см. InboxOptimizedIndexSql).
        builder.HasIndex(x => x.ReceivedAt)
            .HasDatabaseName("IX_InboxMessages_ReceivedAt");
    }
}
