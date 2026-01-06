using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Backend.IdentityCore.DataAccess;

public abstract class IdentityDbContext<TDbContext, TIdentityUser, TIdentityRole>(DbContextOptions<TDbContext> options)
    : CrmDbContext<TDbContext>(options),
        IIdentityDbContext
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
    where TDbContext : DbContext;