using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;
using Cheetah.Identity.Shared.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByEmailQuery, UserViewModel?>))]
public class GetUserByEmailQueryHandler : IQueryHandler<GetUserByEmailQuery, UserViewModel?>
{
    private readonly IIdentityClient _identityClient;

    public GetUserByEmailQueryHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask<UserViewModel?> HandleAsync(GetUserByEmailQuery query, CancellationToken ct)
    {
        return await _identityClient.GetUserByEmailAsync(query.Email, ct);
    }
}
