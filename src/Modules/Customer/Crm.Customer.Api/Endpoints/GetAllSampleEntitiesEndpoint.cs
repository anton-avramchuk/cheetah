using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Customer.Application;
using Crm.Customer.Application.Queries;
using Crm.Customer.Contracts.Requests;
using Crm.Customer.Contracts.Response;

namespace Crm.Customer.Api.Endpoints;

public class GetAllSampleEntitiesEndpoint : QueryGridEndpoint<GetAllSampleEntitiesRequest,
    GetAllSampleEntitiesQuery, CustomerModel, CustomerViewModel>
{
    public override string Route => Constants.DefaultRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}