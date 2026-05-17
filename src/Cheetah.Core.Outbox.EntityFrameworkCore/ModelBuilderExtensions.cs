using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Подключает таблицу OutboxMessages к модели DbContext'a.
    /// Вызывается из OnModelCreating каждого DbContext'a, который должен поддерживать outbox.
    /// </summary>
    public static ModelBuilder AddOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        return modelBuilder;
    }

    /// <summary>
    /// Подключает таблицу InboxMessages к модели DbContext'a.
    /// </summary>
    public static ModelBuilder AddInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        return modelBuilder;
    }

    /// <summary>
    /// Подключает таблицу DeadLetterMessages к модели DbContext'a.
    /// </summary>
    public static ModelBuilder AddDeadLetter(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DeadLetterMessageConfiguration());
        return modelBuilder;
    }
}
