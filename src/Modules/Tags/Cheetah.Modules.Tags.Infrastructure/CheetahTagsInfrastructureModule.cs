using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Client;
using Cheetah.Modules.Tags.Domain;
using Cheetah.Modules.Tags.Infrastructure.Identity;
using Cheetah.Modules.Tags.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Modules.Tags.Infrastructure;

/// <summary>
/// Инфраструктура модуля тэгов: EF Core <see cref="TagsDbContext"/>, миграции и
/// реализации репозиториев (EF). Единственная инфраструктурная сборка модуля.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahTagsDomainModule),
    typeof(CheetahIdentityClientModule))]
public partial class CheetahTagsInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.AddApplicationDbContext<TagsDbContext>();
        services.AddScoped<TagsDbContext>();
        services.AddDatabaseMigrator<TagsDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<TagsDbContext>(); });

        // Bulk-синк реплики пользователей из Identity (через CheetahIdentityClientModule).
        // Адаптер IIdentityUserDirectory регистрируется автогенератором ([Export]).
        // Регистрируется ПОСЛЕ мигратора, чтобы синк шёл по готовой схеме.
        services.AddSingleton<IHostedService, UserDirectorySyncService>();
    }
}
