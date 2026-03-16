using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Cheetah.Modules.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CrmIdentityRole>))]
public class RoleRepository(IdentityModuleDbContext context, IObjectMapper mapper, ILogger<RoleRepository> logger)
    : EfGridRepository<IdentityModuleDbContext, CrmIdentityRole>(context, mapper, logger);
