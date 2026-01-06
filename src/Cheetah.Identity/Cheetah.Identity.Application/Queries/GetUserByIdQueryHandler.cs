using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByIdQuery, UserViewModel?>))]
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserViewModel?>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUserByIdQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<UserViewModel?> HandleAsync(GetUserByIdQuery query, CancellationToken ct)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == query.UserId)
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
