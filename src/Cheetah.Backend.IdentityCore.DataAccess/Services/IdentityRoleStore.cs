using System.Security.Claims;
using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cheetah.Backend.IdentityCore.DataAccess.Services;

public class IdentityRoleStore<TIdentityRole, TIdentityDbContext> : IRoleStore<TIdentityRole>, IRoleClaimStore<TIdentityRole>, IQueryableRoleStore<TIdentityRole>
    where TIdentityRole : IdentityRole
    where TIdentityDbContext : IIdentityDbContext
{
    private readonly TIdentityDbContext _context;
    private readonly ILogger<IdentityRoleStore<TIdentityRole, TIdentityDbContext>> _logger;

    public IdentityRoleStore(TIdentityDbContext context, ILogger<IdentityRoleStore<TIdentityRole, TIdentityDbContext>> logger, IdentityErrorDescriber describer = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ErrorDescriber = describer ?? new IdentityErrorDescriber();
    }
    public IdentityErrorDescriber ErrorDescriber { get; set; }
    public void Dispose()
    {

    }

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

    public Task<string> GetRoleNameAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(TIdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        role.ChangeName(roleName);
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedRoleNameAsync(TIdentityRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(TIdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        // Normalization happens automatically in ChangeName method
        // No need to set it manually as it would violate domain encapsulation
        return Task.CompletedTask;
    }

    public async Task<TIdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        if (roleId == null) throw new ArgumentNullException(nameof(roleId));


        return await _context.Set<TIdentityRole>()
            .FindAsync(Guid.Parse(roleId));
    }

    public async Task<TIdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        if (normalizedRoleName == null) throw new ArgumentNullException(nameof(normalizedRoleName));



        return await _context.Set<TIdentityRole>()
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, cancellationToken);
    }

    public async Task<IList<Claim>> GetClaimsAsync(TIdentityRole role, CancellationToken cancellationToken = new CancellationToken())
    {
        if (role == null) throw new ArgumentNullException(nameof(role));

        await _context.Entry(role).Collection(w => w.Claims).LoadAsync(cancellationToken);
        return role.Claims.Select(x => x.ToClaim()).ToList();
    }

    public async Task AddClaimAsync(TIdentityRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (claim == null) throw new ArgumentNullException(nameof(claim));

        await _context.Entry(role).Collection(w => w.Claims).LoadAsync(cancellationToken);
        role.AddClaim(claim);
    }

    public async Task RemoveClaimAsync(TIdentityRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (claim == null) throw new ArgumentNullException(nameof(claim));

        await _context.Entry(role).Collection(w => w.Claims).LoadAsync(cancellationToken);
        role.RemoveClaim(claim);
    }

    public IQueryable<TIdentityRole> Roles => _context.Set<TIdentityRole>();
}