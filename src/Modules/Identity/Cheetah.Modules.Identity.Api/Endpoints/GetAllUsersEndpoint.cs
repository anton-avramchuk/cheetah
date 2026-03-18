using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class GetAllUsersEndpoint<TRequest, TQuery> : QueryGridEndpoint<TRequest, TQuery, UserModel, UserGridViewModel>
    where TRequest : GetAllUsersRequest
    where TQuery : GetAllUsersQuery
{
    public override string Route => Constants.UsersRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Users");
    }
}
