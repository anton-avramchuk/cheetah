using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.Extensions;
using Cheetah.Core.EntityFramework.Migrations;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.EntityFramework.PostgreSql.Extensions;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Permissions.Catalog.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Permissions.Catalog;

/// <summary>
/// Подключает каталог permissions (БД-проекция объявленных в коде permissions).
/// В монолите запускает <see cref="LocalRegistrySyncService"/> — auto-scan + sync.
///
/// Зависимость: Cheetah.Permissions (core). В микросервисе, где каталог НЕ нужен,
/// этот модуль НЕ подключают — подключают только Cheetah.Permissions + Catalog.Client
/// для отсылки своих permissions по сети.
///
/// <see cref="CrmFeatureManagementModule"/> нужен для <c>ListPermissionsQuery</c>: permissions,
/// привязанные к фиче, прячутся, пока она выключена. Без источника определений флагов модуль
/// подставляет <c>NullFeatureDefinitionProvider</c> — тогда «фичи нет» и такие permissions скрыты.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmFeatureManagementModule),
    typeof(CrmPermissionsModule))]
public partial class CrmPermissionsCatalogModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        RegisterServices(services);

        services.AddApplicationDbContext<PermissionsDbContext>();
        services.AddScoped<PermissionsDbContext>();
        services.AddDatabaseMigrator<PermissionsDbContext>();
        services.Configure<CrmDbContextOptions>(options => { options.UseNpgsql<PermissionsDbContext>(); });

        services.AddSingleton<IHostedService, LocalRegistrySyncService>();
    }
}
