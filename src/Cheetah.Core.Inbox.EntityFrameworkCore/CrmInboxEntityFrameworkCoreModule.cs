using Cheetah.Core;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Inbox.EntityFrameworkCore;

/// <summary>
/// EF Core поддержка Inbox. Регистрация store выполняется явно через
/// services.AddInboxStore&lt;TContext&gt;() в конфигурации модуля-владельца DbContext'a.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmEntityFrameworkModule), typeof(CrmInboxModule))]
public partial class CrmInboxEntityFrameworkCoreModule : CrmModule
{
}
