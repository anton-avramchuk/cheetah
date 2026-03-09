using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application;
using Crm.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Api.Endpoints;

public class GetUserByIdEndpoint : QueryOrNotFoundEndpoint<GetUserByIdRequest, GetUserByIdQuery, UserDetailModel, UserDetailViewModel>
{
    public override string Route => $"{Constants.UsersRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetUserById").WithTags("Users");
    }
}
