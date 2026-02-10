using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetCustomerDirectionByIdEndpoint : QueryOrNotFoundEndpoint<GetCustomerDirectionByIdRequest, GetCustomerDirectionByIdQuery, CustomerDirectionModel,
    CustomerDirectionViewModel>
{
    public override string Route => $"{Constants.CustomerDirectionRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetCustomerDirectionById").WithTags("Customer Directions");
    }
}
