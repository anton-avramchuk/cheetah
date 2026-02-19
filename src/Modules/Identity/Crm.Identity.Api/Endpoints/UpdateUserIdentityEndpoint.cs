using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public class UpdateUserIdentityEndpoint : UpdateCommandEndpoint<UpdateUserIdentityRequest, UpdateUserIdentityCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}