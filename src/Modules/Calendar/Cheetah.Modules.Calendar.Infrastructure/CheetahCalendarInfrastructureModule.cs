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
using Cheetah.Modules.Calendar.Domain;
using Cheetah.Modules.Calendar.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Calendar.Infrastructure;

/// <summary>
/// Инфраструктура Calendar: EF Core <see cref="CalendarDbContext"/>, миграции, репозитории (EF),
/// RRULE-экспандер и Outbox/DeadLetter поверх той же БД для атомарной публикации намерений уведомить.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmOutboxModule),
    typeof(CrmOutboxEntityFrameworkCoreModule),
    typeof(CrmOutboxPostgreSqlModule),
    typeof(CheetahCalendarDomainModule))]
public partial class CheetahCalendarInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.AddApplicationDbContext<CalendarDbContext>();
        services.AddScoped<CalendarDbContext>();
        services.AddDatabaseMigrator<CalendarDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<CalendarDbContext>(); });

        services.AddPostgresOutboxStore<CalendarDbContext>();
        services.AddDeadLetterStore<CalendarDbContext>();
    }
}
