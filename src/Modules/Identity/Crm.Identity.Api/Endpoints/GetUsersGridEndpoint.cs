using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Api.Endpoints;

public sealed class GetUsersGridEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetUsersGridEndpoint<GetUsersGridRequest, GetUsersGridQuery<UserModel>, UserModel, UserGridViewModel>
{
}
