using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Customer.Application.Commands;
using Crm.Customer.Contracts.Requests;

namespace Crm.Customer.Api.Endpoints;

public class CreateCustomerEndpoint : CreateCommandEndpoint<CreateCustomerRequest, CreateCustomerCommand>
{
    public override string Route => Constants.DefaultRoute;

    public override string GetByIdRouteName => "GetCustomerById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}