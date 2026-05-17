using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Audit.EntityFrameworkCore;

/// <summary>
/// EF Core sink для аудита. Регистрация store выполняется явно через
/// services.AddEfAuditSink&lt;TContext&gt;() в конфигурации модуля-владельца DbContext'a.
/// Также при настройке DbContext'a нужно добавить AuditInterceptor:
/// options.AddInterceptors(sp.GetRequiredService&lt;AuditInterceptor&gt;()).
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAuditModule))]
public partial class CrmAuditEntityFrameworkCoreModule : CrmModule
{
}
