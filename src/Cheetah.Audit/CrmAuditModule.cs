using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Audit;

/// <summary>
/// Core-модуль аудита. Подключите дополнительно один из storage-модулей:
/// Cheetah.Audit.EntityFrameworkCore (запись в БД). Для доставки в Kafka — Cheetah.Audit.Kafka.
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmAuditModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        // Дефолтный accessor — заглушка. Прикладной код регистрирует свою реализацию,
        // которая берёт пользователя из HttpContext.
        services.TryAddSingleton<IAuditUserAccessor>(NullAuditUserAccessor.Instance);
        services.AddScoped<AuditInterceptor>();
    }
}
