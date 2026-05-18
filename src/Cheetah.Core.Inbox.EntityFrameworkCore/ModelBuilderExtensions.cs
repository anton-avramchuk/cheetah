using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Inbox.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Подключает таблицу InboxMessages к модели DbContext'a.
    /// Вызывается из OnModelCreating каждого DbContext'a, который должен поддерживать inbox.
    /// </summary>
    public static ModelBuilder AddInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        return modelBuilder;
    }
}
