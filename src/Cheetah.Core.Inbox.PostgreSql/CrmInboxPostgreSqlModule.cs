using Cheetah.Core;
using Cheetah.Core.Inbox.EntityFrameworkCore;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Inbox.PostgreSql;

/// <summary>
/// Подключает Postgres-специфику Inbox: оптимизированные индексы.
/// SQL для создания индекса нужно применять через миграцию (см. InboxOptimizedIndexSql.Create()).
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmInboxModule), typeof(CrmInboxEntityFrameworkCoreModule))]
public partial class CrmInboxPostgreSqlModule : CrmModule
{
}
