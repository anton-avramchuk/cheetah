using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CrmRole>))]
public class CrmRoleRepository(IdentityDbContext context, IObjectMapper mapper, ILogger<CrmRoleRepository> logger)
    : EfGridRepository<IdentityDbContext, CrmRole>(context, mapper, logger);
