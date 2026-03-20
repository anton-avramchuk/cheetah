using AppName.Identity.Contracts.Requests;
using AppName.Identity.Contracts.Response;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetUsersGridEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetUsersGridEndpoint<GetUsersGridRequest, GetUsersGridQuery<UserModel>, UserModel, UserGridViewModel>
{
}
