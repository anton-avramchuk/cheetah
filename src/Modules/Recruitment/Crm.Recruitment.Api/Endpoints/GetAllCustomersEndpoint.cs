using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetAllCustomersEndpoint : QueryGridEndpoint<GetAllCustomersRequest,
    GetAllCustomersQuery, CustomerModel, CustomerViewModel>
{
    public override string Route => Constants.CustomerRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Customers");
    }
}
