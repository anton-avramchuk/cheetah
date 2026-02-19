using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public class CreateUserIdentityEndpoint : CreateCommandEndpoint<CreateUserIdentityRequest, CreateUserIdentityCommand>
{
    public override string Route => Constants.DefaultRoute;

    public override string GetByIdRouteName => "GetUserIdentityById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}