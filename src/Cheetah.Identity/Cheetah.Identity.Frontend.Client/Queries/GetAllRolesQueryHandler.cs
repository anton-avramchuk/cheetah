using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Client;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllRolesQuery, List<RoleViewModel>>))]
public class GetAllRolesQueryHandler : IQueryHandler<GetAllRolesQuery, List<RoleViewModel>>
{
    private readonly IIdentityClient _identityClient;

    public GetAllRolesQueryHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async ValueTask<List<RoleViewModel>> HandleAsync(GetAllRolesQuery query, CancellationToken ct)
    {
        return await _identityClient.GetAllRolesAsync(ct);
    }
}
