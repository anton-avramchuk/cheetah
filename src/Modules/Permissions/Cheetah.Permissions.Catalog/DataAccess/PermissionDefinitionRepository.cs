using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Permissions.Catalog.Domain;

namespace Cheetah.Permissions.Catalog.DataAccess;

[Export(LifetimeType.Scoped, typeof(IRepository<PermissionDefinition, string>))]
public class PermissionDefinitionRepository : EfRepository<PermissionsDbContext, PermissionDefinition, string>
{
    public PermissionDefinitionRepository(PermissionsDbContext context) : base(context) { }
}
