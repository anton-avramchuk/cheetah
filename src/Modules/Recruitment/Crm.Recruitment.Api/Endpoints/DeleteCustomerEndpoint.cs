using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class DeleteCustomerEndpoint : DeleteCommandEndpoint<DeleteCustomerRequest, DeleteCustomerCommand>
{
    public override string Route => $"{Constants.CustomerRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Customers");
    }
}
