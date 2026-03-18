using AppName.Application.Commands;
using AppName.Contracts.Requests;
using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;

namespace AppName.Api.Endpoints;

public class CreateSampleEntityEndpoint : CreateCommandEndpoint<CreateSampleEntityRequest, CreateSampleEntityCommand>
{
    public override string Route => Constants.DefaultRoute;

    public override string GetByIdRouteName => "GetSampleEntityById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}
