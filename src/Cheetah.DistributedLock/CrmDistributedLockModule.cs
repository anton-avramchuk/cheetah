using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.DistributedLock;

/// <summary>
/// Core-модуль distributed-блокировок. Только абстракции. Реализация подключается отдельным
/// модулем — например Cheetah.DistributedLock.Postgres (pg_advisory_lock).
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmDistributedLockModule : CrmModule
{
}
