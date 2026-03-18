using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class GetUsersGridEndpoint<TRequest, TQuery, TUserModel, TUserGridViewModel>
    : QueryGridEndpoint<TRequest, TQuery, TUserModel, TUserGridViewModel>
    where TRequest : GetUsersGridRequest
    where TQuery : GetUsersGridQuery<TUserModel>
    where TUserModel : UserModel
    where TUserGridViewModel : UserGridViewModel
{
    public override string Route => Constants.UsersRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Users");
    }
}
