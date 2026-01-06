using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Identity Core Events Module
/// Contains domain events for Identity Core module
/// Other modules can depend on this to subscribe to identity events
/// </summary>
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class IdentityCoreEventsModule : CrmModule;
