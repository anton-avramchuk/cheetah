using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework.Providers;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.EntityFramework;

[DependsOn(typeof(CrmDomainModule),typeof(CrmDataAccessModule))]
public partial class CrmEntityFrameworkModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.TryAddTransient(typeof(IDbContextProvider<>), typeof(DbContextProvider<>));
    }
}