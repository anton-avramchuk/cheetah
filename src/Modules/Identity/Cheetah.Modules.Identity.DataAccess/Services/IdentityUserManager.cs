using Cheetah.Core.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Identity.DataAccess.Services;

public class IdentityUserManager<TIdentityUser, TIdentityRole>(
    IUserStore<TIdentityUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<TIdentityUser> passwordHasher,
    IEnumerable<IUserValidator<TIdentityUser>> userValidators,
    IEnumerable<IPasswordValidator<TIdentityUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<TIdentityUser>> logger)
    : UserManager<TIdentityUser>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
        keyNormalizer, errors, services, logger)
    where TIdentityRole : IdentityRole
    where TIdentityUser : IdentityUser<TIdentityRole>;
