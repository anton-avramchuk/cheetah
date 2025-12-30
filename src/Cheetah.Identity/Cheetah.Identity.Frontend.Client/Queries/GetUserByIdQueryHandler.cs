using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;
using Cheetah.Identity.Shared.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByIdQuery, UserViewModel?>))]
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserViewModel?>
{
    private readonly IIdentityClient _identityClient;

    public GetUserByIdQueryHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask<UserViewModel?> HandleAsync(GetUserByIdQuery query, CancellationToken ct)
    {
        return await _identityClient.GetUserByIdAsync(query.UserId, ct);
    }
}
