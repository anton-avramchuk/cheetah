using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.BackgroundTasks;

[DependsOn(typeof(CoreModule), typeof(CrmDataAccessModule))]
public partial class CrmBackgroundTasksModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddHostedService<BackgroundTaskScheduler>();
    }
}
