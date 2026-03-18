using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CrmIdentityUser>))]
public class UserRepository(IdentityModuleDbContext context, IObjectMapper mapper, ILogger<UserRepository> logger)
    : EfGridRepository<IdentityModuleDbContext, CrmIdentityUser>(context, mapper, logger);
