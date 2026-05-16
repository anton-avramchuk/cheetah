using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Core.Outbox.PostgreSql;

/// <summary>
/// Подключает Postgres LISTEN/NOTIFY-нотификатор для OutboxProcessor.
/// Регистрирует PostgresOutboxNotifier как IOutboxNotifier (singleton) и как BackgroundService.
/// Polling в OutboxProcessor остаётся как fallback — НЕ убирать.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmOutboxModule), typeof(CrmOutboxEntityFrameworkCoreModule))]
public partial class CrmOutboxPostgreSqlModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();
        services.Configure<PostgresOutboxOptions>(configuration.GetSection("Outbox:Postgres"));

        RegisterServices(services);

        // Singleton: один долгоживущий коннект на инстанс приложения.
        services.AddSingleton<PostgresOutboxNotifier>();

        // Перекрываем заглушку из CrmOutboxModule.
        services.Replace(ServiceDescriptor.Singleton<IOutboxNotifier>(
            sp => sp.GetRequiredService<PostgresOutboxNotifier>()));

        // BackgroundService для самого notifier'а — держит LISTEN-коннект.
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<PostgresOutboxNotifier>());
    }
}
