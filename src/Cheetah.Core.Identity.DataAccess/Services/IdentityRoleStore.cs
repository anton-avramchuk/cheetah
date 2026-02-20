using System.Security.Claims;
using Cheetah.Core.Extensions.Logging;
using Cheetah.Core.Identity.DataAccess.Context;
using Cheetah.Core.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.Identity.DataAccess.Services;

public class IdentityRoleStore<TIdentityRole, TIdentityDbContext>(
    TIdentityDbContext context,
    ILogger<IdentityRoleStore<TIdentityRole, TIdentityDbContext>> logger,
    IdentityErrorDescriber? describer = null)
    : IRoleStore<TIdentityRole>, IRoleClaimStore<TIdentityRole>, IQueryableRoleStore<TIdentityRole>
    where TIdentityRole : IdentityRole
    where TIdentityDbContext : IIdentityDbContext
{
    private readonly TIdentityDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<IdentityRoleStore<TIdentityRole, TIdentityDbContext>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public IdentityErrorDescriber ErrorDescriber { get; set; } = describer ?? new IdentityErrorDescriber();

    public void Dispose() { }

    public async Task<IdentityResult> CreateAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        await _context.Set<TIdentityRole>().AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        try
        {
            _context.Update(role);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogException(e);
            return IdentityResult.Failed(ErrorDescriber.DefaultError());
        }
    }

    public async Task<IdentityResult> DeleteAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        try
        {
            _context.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogException(e);
            return IdentityResult.Failed(ErrorDescriber.DefaultError());
        }
    }

    public Task<string> GetRoleIdAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string?> GetRoleNameAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        return Task.FromResult<string?>(role.Name);
    }

    public Task SetRoleNameAsync(TIdentityRole role, string? roleName, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (roleName != null)
            role.ChangeName(roleName);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        return Task.FromResult<string?>(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(TIdentityRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        // Normalized name is managed automatically by the domain entity via ChangeName
        return Task.CompletedTask;
    }

    public async Task<TIdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        if (roleId == null) throw new ArgumentNullException(nameof(roleId));
        return await _context.Set<TIdentityRole>().FindAsync([Guid.Parse(roleId)], cancellationToken);
    }

    public async Task<TIdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        if (normalizedRoleName == null) throw new ArgumentNullException(nameof(normalizedRoleName));
        return await _context.Set<TIdentityRole>()
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, cancellationToken);
    }

    public async Task<IList<Claim>> GetClaimsAsync(TIdentityRole role, CancellationToken cancellationToken = default)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        await _context.Entry(role).Collection(w => w.Claims).LoadAsync(cancellationToken);
        return role.Claims.Select(x => x.ToClaim()).ToList();
    }

    public async Task AddClaimAsync(TIdentityRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (claim == null) throw new ArgumentNullException(nameof(claim));
        await _context.Entry(role).Collection(w => w.Claims).LoadAsync(cancellationToken);
        role.AddClaim(claim);
    }

    public async Task RemoveClaimAsync(TIdentityRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (claim == null) throw new ArgumentNullException(nameof(claim));
        await _context.Entry(role).Collection(w => w.Claims).LoadAsync(cancellationToken);
        role.RemoveClaim(claim);
    }

    public IQueryable<TIdentityRole> Roles => _context.Set<TIdentityRole>();
}
