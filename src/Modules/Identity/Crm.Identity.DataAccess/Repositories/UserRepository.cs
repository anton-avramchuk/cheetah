using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CrmIdentityUser>))]
public class UserRepository(IdentityDbContext context, IObjectMapper mapper, ILogger<UserRepository> logger)
    : EfGridRepository<IdentityDbContext, CrmIdentityUser>(context, mapper, logger);
