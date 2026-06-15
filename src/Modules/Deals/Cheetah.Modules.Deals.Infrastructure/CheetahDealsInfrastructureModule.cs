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
using Cheetah.Modules.Deals.Domain;
using Cheetah.Modules.Deals.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Deals.Infrastructure;

/// <summary>
/// Инфраструктура Deals: EF Core <see cref="DealsDbContext"/>, миграции, репозитории (EF) и
/// Outbox/DeadLetter поверх той же БД — интеграционные события сделок ложатся в OutboxMessages
/// в той же транзакции, что и сохранение агрегата (атомарность «сохранил + опубликовал»).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmOutboxModule),
    typeof(CrmOutboxEntityFrameworkCoreModule),
    typeof(CrmOutboxPostgreSqlModule),
    typeof(CheetahDealsDomainModule))]
public partial class CheetahDealsInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.AddApplicationDbContext<DealsDbContext>();
        services.AddScoped<DealsDbContext>();
        services.AddDatabaseMigrator<DealsDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<DealsDbContext>(); });

        services.AddPostgresOutboxStore<DealsDbContext>();
        services.AddDeadLetterStore<DealsDbContext>();
    }
}
