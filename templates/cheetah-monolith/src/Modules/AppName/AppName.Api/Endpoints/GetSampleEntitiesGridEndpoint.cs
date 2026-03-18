using AppName.Application;
using AppName.Application.Queries;
using AppName.Contracts.Requests;
using AppName.Contracts.Response;
using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;

namespace AppName.Api.Endpoints;

public class GetSampleEntitiesGridEndpoint : QueryGridEndpoint<GetSampleEntitiesGridRequest,
    GetSampleEntitiesGridQuery, SampleEntityModel, SampleEntityViewModel>
{
    public override string Route => Constants.DefaultRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}
