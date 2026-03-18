using AppName.Application;
using AppName.Application.Queries;
using AppName.Contracts.Requests;
using AppName.Contracts.Response;
using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;

namespace AppName.Api.Endpoints;

public class GetSampleEntityByIdEndpoint : QueryOrNotFoundEndpoint<GetSampleEntityByIdRequest, GetSampleEntityByIdQuery, SampleEntityModel, SampleEntityViewModel>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetSampleEntityById");
        config.WithTags("SampleEntities");
    }
}
