using System.Globalization;
using System.Security.Claims;
using Cheetah.Core.Identity.DataAccess.Context;
using Cheetah.Core.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Identity.DataAccess.Services;

public sealed class IdentityUserStore<TIdentityUser, TIdentityRole, TIdentityContext> :
        IUserStore<TIdentityUser>,
        IUserEmailStore<TIdentityUser>,
        IUserRoleStore<TIdentityUser>,
        IUserClaimStore<TIdentityUser>,
        IUserPasswordStore<TIdentityUser>,
        IUserSecurityStampStore<TIdentityUser>,
        IQueryableUserStore<TIdentityUser>
    where TIdentityUser : IdentityUser<TIdentityRole> 
    where TIdentityContext : IIdentityDbContext 
    where TIdentityRole : IdentityRole
{
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IRoleStore<TIdentityRole> _roleStore;
    private readonly TIdentityContext _context;

    public IdentityErrorDescriber ErrorDescriber { get; set; }
    private ILogger<IdentityUserStore<TIdentityUser, TIdentityRole, TIdentityContext>> Logger { get; }

    public IdentityUserStore(
        ILookupNormalizer lookupNormalizer,
        IRoleStore<TIdentityRole> roleStore,
        TIdentityContext context, ILogger<IdentityUserStore<TIdentityUser, TIdentityRole, TIdentityContext>> logger,
    IdentityErrorDescriber? describer = null)
    {
        _lookupNormalizer = lookupNormalizer;
        _roleStore = roleStore;
        _context = context;
        Logger = logger;
        ErrorDescriber = describer ?? new IdentityErrorDescriber();
    }


    public void Dispose()
    {
        // Context is Scoped and owned by DI — must not be disposed here
    }

    public Task<string> GetUserIdAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(TIdentityUser user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (userName == null) throw new ArgumentNullException(nameof(userName));

        user.ChangeUserName(userName);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(TIdentityUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        // Normalized name is managed automatically by the domain entity via ChangeUserName
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            _context.Update(user);
            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception e)
        {
            Logger.LogWarning(e.ToString());
            return IdentityResult.Failed(ErrorDescriber.DefaultError());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            _context.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception e)
        {
            Logger.LogWarning(e.ToString());
            return IdentityResult.Failed(ErrorDescriber.DefaultError());
        }

        return IdentityResult.Success;
    }

    public async Task<TIdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _context.Set<TIdentityUser>().FindAsync([Guid.Parse(userId)], cancellationToken);
    }

    public async Task<TIdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (normalizedUserName == null) throw new ArgumentNullException(nameof(normalizedUserName));

        return await _context.Set<TIdentityUser>().FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken);

    }


    public Task SetEmailAsync(TIdentityUser user, string? email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (email == null) throw new ArgumentNullException(nameof(email));
        user.ChangeEmail(email);
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(TIdentityUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        user.SetEmailConfirmed(confirmed);
        return Task.CompletedTask;
    }

    public async Task<TIdentityUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (normalizedEmail == null) throw new ArgumentNullException(nameof(normalizedEmail));

        return await _context.Set<TIdentityUser>().FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);


    }

    public Task<string?> GetNormalizedEmailAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(TIdentityUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        // Normalized email is managed automatically by the domain entity via ChangeEmail
        return Task.CompletedTask;
    }

    public async Task AddToRoleAsync(TIdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (roleName == null) throw new ArgumentNullException(nameof(roleName));
        if (await IsInRoleAsync(user, roleName, cancellationToken))
        {
            return;
        }

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist!", roleName));
        }

        await _context.Entry(user).Collection(x => x.Roles).LoadAsync(cancellationToken);
        user.AddRole(role);


    }

    public async Task RemoveFromRoleAsync(TIdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (roleName == null) throw new ArgumentNullException(nameof(roleName));
        cancellationToken.ThrowIfCancellationRequested();

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);

        if (role == null)
            return;

        await _context.Entry(user).Collection(x => x.Roles).LoadAsync(cancellationToken);
        user.RemoveRole(role);

    }
    public async Task<IList<string>> GetRolesAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();

        return await (from userRole in _context.Set<IdentityUserRole<TIdentityRole>>()
                      join role in _context.Set<TIdentityRole>() on userRole.RoleId equals role.Id
                      where userRole.UserId == user.Id
                      where role.Name != null
                      select role.Name!).ToListAsync(cancellationToken);

    }

    public async Task<bool> IsInRoleAsync(TIdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (roleName == null) throw new ArgumentNullException(nameof(roleName));
        cancellationToken.ThrowIfCancellationRequested();

        var roles = await GetRolesAsync(user, cancellationToken);

        return roles
            .Select(r => _lookupNormalizer.NormalizeName(r))
            .Contains(roleName);
    }

    public async Task<IList<TIdentityUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (roleName == null) throw new ArgumentNullException(nameof(roleName));
        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            return new List<TIdentityUser>();
        }

        return await _context.Set<TIdentityUser>().Where(x => x.Roles.Any(q => q.RoleId == role.Id))
            .ToListAsync(cancellationToken);

    }

    public async Task<IList<Claim>> GetClaimsAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));

        await _context.Entry(user).Collection(x => x.Claims).LoadAsync(cancellationToken);
        return user.Claims.Select(x => x.ToClaim()).ToList();

    }

    public async Task AddClaimsAsync(TIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (claims == null) throw new ArgumentNullException(nameof(claims));

        await _context.Entry(user).Collection(x => x.Claims).LoadAsync(cancellationToken);
        user.AddClaims(claims);
    }

    public async Task ReplaceClaimAsync(TIdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (claim == null) throw new ArgumentNullException(nameof(claim));
        if (newClaim == null) throw new ArgumentNullException(nameof(newClaim));

        await _context.Entry(user).Collection(x => x.Claims).LoadAsync(cancellationToken);
        user.ReplaceClaim(claim, newClaim);

    }

    public async Task RemoveClaimsAsync(TIdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (claims == null) throw new ArgumentNullException(nameof(claims));

        await _context.Entry(user).Collection(x => x.Claims).LoadAsync(cancellationToken);
        user.RemoveClaims(claims);
    }

    public async Task<IList<TIdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (claim == null) throw new ArgumentNullException(nameof(claim));
        return await _context.Set<TIdentityUser>()
            .Where(x => x.Claims.Any(q => q.ClaimType == claim.Type && q.ClaimValue == claim.Value))
            .ToListAsync(cancellationToken);

    }

    public Task SetPasswordHashAsync(TIdentityUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();

        user.SetPasswordHash(passwordHash);

        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash != null);
    }

    public Task SetSecurityStampAsync(TIdentityUser user, string stamp, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();
        user.SetSecurityStamp(stamp);
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(TIdentityUser user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.SecurityStamp);
    }

    public IQueryable<TIdentityUser> Users
    {
        get
        {
            return _context.Set<TIdentityUser>();
        }
    }
}