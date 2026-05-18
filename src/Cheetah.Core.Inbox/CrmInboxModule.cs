using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Core.Inbox;

/// <summary>
/// Подключает Inbox-паттерн: идемпотентная обработка входящих событий.
///
/// Что делает:
/// 1) биндит InboxOptions из секции "Inbox";
/// 2) регистрирует null-метрику (заменяется OpenTelemetry-обёрткой при необходимости);
/// 3) запускает InboxCleanupService для периодической чистки таблицы.
///
/// Реализация IInboxStore подключается отдельным модулем (например, Cheetah.Core.Inbox.EntityFrameworkCore).
/// Регистрация декораторов InboxIdempotentEventHandler для [Idempotent]-handler'ов
/// выполняется Source Generator'ом (Cheetah.Generators.Module).
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmInboxModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();
        services.Configure<InboxOptions>(configuration.GetSection("Inbox"));

        RegisterServices(services);

        services.TryAddSingleton<IInboxMetrics>(NullInboxMetrics.Instance);
        services.AddSingleton<IHostedService, InboxCleanupService>();
    }
}
