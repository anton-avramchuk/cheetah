using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using __Prefix__.ModuleName.Application;
using __Prefix__.ModuleName.Application.Queries;
using __Prefix__.ModuleName.Contracts.Requests;
using __Prefix__.ModuleName.Contracts.Response;

namespace __Prefix__.ModuleName.Api.Endpoints;

public class GetAllSampleEntitiesEndpoint : QueryGridEndpoint<GetAllSampleEntitiesRequest,
    GetAllSampleEntitiesQuery, SampleEntityModel, SampleEntityViewModel>
{
    public override string Route => Constants.DefaultRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}
