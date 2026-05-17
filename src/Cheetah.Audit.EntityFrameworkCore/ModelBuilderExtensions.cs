using Microsoft.EntityFrameworkCore;

namespace Cheetah.Audit.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Подключает таблицу AuditEntries к модели DbContext'a.
    /// Вызывается из OnModelCreating каждого DbContext'a, который должен сохранять audit.
    /// </summary>
    public static ModelBuilder AddAudit(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AuditEntryConfiguration());
        return modelBuilder;
    }
}
