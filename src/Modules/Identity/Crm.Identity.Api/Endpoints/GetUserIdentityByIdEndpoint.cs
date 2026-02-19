using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application;
using Crm.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Api.Endpoints;

public class GetUserIdentityByIdEndpoint : QueryOrNotFoundEndpoint<GetUserIdentityByIdRequest, GetUserIdentityByIdQuery,
    UserIdentityModel, UserIdentityViewModel>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetUserIdentityById");
        config.WithTags("SampleEntities");
    }
}