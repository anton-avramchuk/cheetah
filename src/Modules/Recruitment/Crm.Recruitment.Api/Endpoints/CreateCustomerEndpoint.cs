using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateCustomerEndpoint : CreateCommandEndpoint<CreateCustomerRequest, CreateCustomerCommand>
{
    public override string Route => Constants.CustomerRoute;

    public override string GetByIdRouteName => "GetCustomerById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Customers");
    }
}
