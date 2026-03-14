using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.BackgroundTasks;

/// <summary>
/// Registers the background task infrastructure.
/// Depends only on <see cref="CoreModule"/>.
/// </summary>
[DependsOn(typeof(CoreModule))]
public partial class CrmBackgroundTasksModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddHostedService<BackgroundTaskScheduler>();
    }
}
