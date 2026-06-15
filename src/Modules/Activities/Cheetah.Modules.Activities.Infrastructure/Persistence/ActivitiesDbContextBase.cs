using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Activities.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Activities.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля Activities. Хостит активности
/// (<typeparamref name="TActivity"/>) и их напоминания в одной БД. Наследник закрывает его
/// конкретным типом активности:
/// <c>class AppActivitiesDbContext : ActivitiesDbContextBase&lt;AppActivitiesDbContext, Activity&gt;</c>
/// и поставляет конфигурацию активности через <see cref="CreateActivityConfiguration"/>. Миграции —
/// у наследника. Имя подключения по умолчанию — <see cref="ActivitiesConstants.ConnectionStringName"/>.
/// </summary>
[ConnectionStringName(ActivitiesConstants.ConnectionStringName)]
public abstract class ActivitiesDbContextBase<TContext, TActivity> : CrmDbContext<TContext>
    where TContext : DbContext
    where TActivity : ActivityBase
{
    public DbSet<TActivity> Activities => Set<TActivity>();
    public DbSet<ActivityReminder> ActivityReminders => Set<ActivityReminder>();

    protected ActivitiesDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateActivityConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityReminderConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности активности, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TActivity> CreateActivityConfiguration();
}
