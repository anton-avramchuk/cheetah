using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace AppName.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<AppNameIdentityUser>))]
public class UserRepository(IdentityDbContext context, IObjectMapper mapper, ILogger<UserRepository> logger)
    : EfGridRepository<IdentityDbContext, AppNameIdentityUser>(context, mapper, logger);
