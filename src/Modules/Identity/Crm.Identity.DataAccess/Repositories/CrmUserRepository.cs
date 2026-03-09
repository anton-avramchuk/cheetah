using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CrmUser>))]
public class CrmUserRepository(IdentityDbContext context, IObjectMapper mapper, ILogger<CrmUserRepository> logger)
    : EfGridRepository<IdentityDbContext, CrmUser>(context, mapper, logger);
