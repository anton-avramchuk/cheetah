using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class UpdateCustomerDirectionEndpoint : UpdateCommandEndpoint<UpdateCustomerDirectionRequest, UpdateCustomerDirectionCommand>
{
    public override string Route => $"{Constants.CustomerDirectionRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Customer Directions");
    }
}
