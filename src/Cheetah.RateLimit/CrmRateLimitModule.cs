using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.RateLimit;

/// <summary>
/// Core-модуль абстракций rate limiting. Реализация подключается отдельным модулем:
/// Cheetah.RateLimit.Redis для distributed sliding window.
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmRateLimitModule : CrmModule
{
}
