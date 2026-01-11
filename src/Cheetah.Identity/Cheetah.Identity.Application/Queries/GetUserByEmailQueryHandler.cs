using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Contracts.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByEmailQuery, UserViewModel?>))]
public class GetUserByEmailQueryHandler : IQueryHandler<GetUserByEmailQuery, UserViewModel?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async ValueTask<UserViewModel?> HandleAsync(GetUserByEmailQuery query, CancellationToken ct)
    {
        var normalizedEmail = query.Email.ToUpperInvariant();

        return await _userRepository.AsNoTrackingQueryable()
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
