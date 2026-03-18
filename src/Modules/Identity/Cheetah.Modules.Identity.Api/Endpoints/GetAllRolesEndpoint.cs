using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class GetAllRolesEndpoint<TRequest, TQuery> : QueryGridEndpoint<TRequest, TQuery, RoleModel, RoleViewModel>
    where TRequest : GetAllRolesRequest
    where TQuery : GetAllRolesQuery
{
    public override string Route => Constants.RolesRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Roles");
    }
}
