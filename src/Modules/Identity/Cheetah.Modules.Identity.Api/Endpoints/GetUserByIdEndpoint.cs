using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class GetUserByIdEndpoint<TRequest, TQuery> : QueryOrNotFoundEndpoint<TRequest, TQuery, UserDetailModel, UserDetailViewModel>
    where TRequest : GetUserByIdRequest
    where TQuery : GetUserByIdQuery
{
    public override string Route => $"{Constants.UsersRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetUserById").WithTags("Users");
    }
}
