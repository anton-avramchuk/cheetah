using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Customer.Application.Commands;
using Crm.Customer.Contracts.Requests;

namespace Crm.Customer.Api.Endpoints;

public class DeleteCustomerEndpoint : DeleteCommandEndpoint<DeleteCustomerRequest, DeleteCustomerCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}