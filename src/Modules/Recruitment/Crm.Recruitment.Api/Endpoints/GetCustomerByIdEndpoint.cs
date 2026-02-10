using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetCustomerByIdEndpoint : QueryOrNotFoundEndpoint<GetCustomerByIdRequest, GetCustomerByIdQuery, CustomerModel,
    CustomerViewModel>
{
    public override string Route => $"{Constants.CustomerRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetCustomerById").WithTags("Customers");
    }
}
