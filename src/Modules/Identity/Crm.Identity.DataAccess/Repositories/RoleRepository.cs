using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CrmIdentityRole>))]
public class RoleRepository(IdentityDbContext context, IObjectMapper mapper, ILogger<RoleRepository> logger)
    : EfGridRepository<IdentityDbContext, CrmIdentityRole>(context, mapper, logger);
