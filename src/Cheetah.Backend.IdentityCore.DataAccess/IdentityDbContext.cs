using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Backend.IdentityCore.DataAccess;

public abstract class IdentityDbContext<TDbContext, TIdentityUser, TIdentityRole> : CrmDbContext<TDbContext>,
    IIdentityDbContext
    where TIdentityUser : IdentityUser<TIdentityRole> where TIdentityRole : IdentityRole where TDbContext : DbContext
{
    protected IdentityDbContext(DbContextOptions<TDbContext> options) : base(options)
    {
    }

}