using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetAllCustomerDirectionsEndpoint : QueryCollectionEndpoint<GetAllCustomerDirectionsRequest,
    GetAllCustomerDirectionsQuery, CustomerDirectionModel, CustomerDirectionViewModel>
{
    public override string Route => Constants.CustomerDirectionRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Customer Directions");
    }
}
