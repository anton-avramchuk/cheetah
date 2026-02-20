using System;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Identity.Domain;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Crm.Identity.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<UserIdentity>), typeof(IRepository<UserIdentity, Guid>))]
public class UserIdentityRepository : EfGridRepository<IdentityDbContext, UserIdentity>
{
    public UserIdentityRepository(IdentityDbContext context, IObjectMapper mapper,
        ILogger<UserIdentityRepository> logger)
        : base(context, mapper, logger)
    {
    }
}