using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace AppName.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<AppNameIdentityRole>))]
public class RoleRepository(IdentityDbContext context, IObjectMapper mapper, ILogger<RoleRepository> logger)
    : EfGridRepository<IdentityDbContext, AppNameIdentityRole>(context, mapper, logger);
