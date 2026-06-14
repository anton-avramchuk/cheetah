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
using Cheetah.Modules.Email.Domain;
using Cheetah.Modules.Email.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Email.Infrastructure;

/// <summary>
/// Инфраструктура Email: EF Core <see cref="EmailDbContext"/>, миграции, репозиторий маркеров
/// и реализация шлюза. В слайсе шлюз — <see cref="Gateways.LoggingEmailGateway"/> (логирует вместо
/// реальной отправки). Реальный SMTP/SES-адаптер + per-provider rate limiting — follow-up.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmOutboxModule),
    typeof(CrmOutboxEntityFrameworkCoreModule),
    typeof(CrmOutboxPostgreSqlModule),
    typeof(CheetahEmailDomainModule))]
public partial class CheetahEmailInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.AddApplicationDbContext<EmailDbContext>();
        services.AddScoped<EmailDbContext>();
        services.AddDatabaseMigrator<EmailDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<EmailDbContext>(); });

        // Outbox/DeadLetter поверх EmailDbContext: обратные статусы публикуются транзакционно.
        // PostgresOutboxStore использует SKIP LOCKED — корректно при нескольких репликах.
        services.AddPostgresOutboxStore<EmailDbContext>();
        services.AddDeadLetterStore<EmailDbContext>();
    }
}
