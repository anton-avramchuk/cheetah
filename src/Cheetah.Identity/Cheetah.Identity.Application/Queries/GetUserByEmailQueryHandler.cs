using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByEmailQuery, UserViewModel?>))]
public class GetUserByEmailQueryHandler : IQueryHandler<GetUserByEmailQuery, UserViewModel?>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUserByEmailQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<UserViewModel?> HandleAsync(GetUserByEmailQuery query, CancellationToken ct)
    {
        var normalizedEmail = query.Email.ToUpperInvariant();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.NormalizedEmail == normalizedEmail)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }
}
