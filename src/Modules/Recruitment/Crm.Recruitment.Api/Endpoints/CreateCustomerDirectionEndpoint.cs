using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateCustomerDirectionEndpoint : CreateCommandEndpoint<CreateCustomerDirectionRequest, CreateCustomerDirectionCommand>
{
    public override string Route => Constants.CustomerDirectionRoute;

    public override string GetByIdRouteName => "GetCustomerDirectionById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Customer Directions");
    }
}
