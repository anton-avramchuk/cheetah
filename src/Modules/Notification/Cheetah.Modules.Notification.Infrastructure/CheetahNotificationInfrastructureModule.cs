using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Cheetah.Core.Outbox.PostgreSql;
using Cheetah.Modules.Notification.Domain;
using Cheetah.Modules.Notification.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Notification.Infrastructure;

/// <summary>
/// Инфраструктура Notification: EF Core <see cref="NotificationsDbContext"/>, миграции,
/// репозитории (EF) и Scriban-рендерер шаблонов. Единственная инфраструктурная сборка модуля.
///
/// <para>Контакты НЕ синкаются bulk'ом из Identity (в отличие от Tags), а наполняются
/// доменными событиями Identity — UserCreatedEvent несёт email. Начальный bulk-sync контактов
/// — follow-up (нужен directory-порт, отдающий email).</para>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmOutboxModule),
    typeof(CrmOutboxEntityFrameworkCoreModule),
    typeof(CrmOutboxPostgreSqlModule),
    typeof(CheetahNotificationDomainModule))]
public partial class CheetahNotificationInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.AddApplicationDbContext<NotificationsDbContext>();
        services.AddScoped<NotificationsDbContext>();
        services.AddDatabaseMigrator<NotificationsDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<NotificationsDbContext>(); });

        // Outbox/DeadLetter поверх NotificationsDbContext: события публикуются транзакционно.
        // PostgresOutboxStore использует SKIP LOCKED — корректно при нескольких репликах.
        services.AddPostgresOutboxStore<NotificationsDbContext>();
        services.AddDeadLetterStore<NotificationsDbContext>();
    }
}
