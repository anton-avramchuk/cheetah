using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.CustomFields.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.CustomFields.Infrastructure.Persistence;

/// <summary>
/// БД модуля CustomFields: каталог типов, определения полей и наборы значений (jsonb).
/// Интеграционные события публикуются через <c>IEventBus</c> после <c>SaveChangesAsync</c>
/// (без Outbox в MVP — апгрейд до транзакционного Outbox является follow-up).
/// </summary>
[ConnectionStringName(CustomFieldsConstants.ConnectionStringName)]
public class CustomFieldsDbContext(DbContextOptions<CustomFieldsDbContext> options)
    : CrmDbContext<CustomFieldsDbContext>(options)
{
    public DbSet<CustomFieldEntityType> CustomFieldEntityTypes => Set<CustomFieldEntityType>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<CustomFieldValueSet> CustomFieldValueSets => Set<CustomFieldValueSet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CustomFieldEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CustomFieldDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomFieldValueSetConfiguration());
    }
}
