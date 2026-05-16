using Cheetah.Core;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Outbox.EntityFrameworkCore;

/// <summary>
/// EF Core поддержка Outbox/Inbox. Регистрация store-ов выполняется явно через
/// services.AddOutboxStore&lt;TContext&gt;() / AddInboxStore&lt;TContext&gt;()
/// в конфигурации модуля-владельца DbContext'a.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEntityFrameworkModule), typeof(CrmOutboxModule))]
public partial class CrmOutboxEntityFrameworkCoreModule : CrmModule
{
}
