using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class GetRoleByIdEndpoint<TRequest, TQuery> : QueryOrNotFoundEndpoint<TRequest, TQuery, RoleModel, RoleViewModel>
    where TRequest : GetRoleByIdRequest
    where TQuery : GetRoleByIdQuery
{
    public override string Route => $"{Constants.RolesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetRoleById").WithTags("Roles");
    }
}
