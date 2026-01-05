using Cheetah.Backend.IdentityCore.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Cheetah.Backend.IdentityCore.DataAccess.Services;

public class IdentityRoleManager<TIdentityRole>(
    IRoleStore<TIdentityRole> store,
    IEnumerable<IRoleValidator<TIdentityRole>> roleValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    ILogger<RoleManager<TIdentityRole>> logger)
    : RoleManager<TIdentityRole>(store, roleValidators, keyNormalizer, errors, logger)
    where TIdentityRole : IdentityRole
{
    public virtual async Task<TIdentityRole> GetByIdAsync(Guid id)
    {
        var role = await Store.FindByIdAsync(id.ToString(), CancellationToken);
        if (role == null)
        {
            throw new NotImplementedException();
            //throw new NotFoundException(typeof(TIdentityRole), id);
        }

        return role;
    }
      
}