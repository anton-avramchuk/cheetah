using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;

namespace Cheetah.Identity.Frontend.Client.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserPermissionsQuery, List<string>>))]
public class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, List<string>>
{
    private readonly IIdentityClient _identityClient;

    public GetUserPermissionsQueryHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask<List<string>> HandleAsync(GetUserPermissionsQuery query, CancellationToken ct)
    {
        return await _identityClient.GetUserPermissionsAsync(query.UserId, ct);
    }
}
