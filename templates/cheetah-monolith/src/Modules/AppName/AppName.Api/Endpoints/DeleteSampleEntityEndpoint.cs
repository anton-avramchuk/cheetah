using AppName.Application.Commands;
using AppName.Contracts.Requests;
using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;

namespace AppName.Api.Endpoints;

public class DeleteSampleEntityEndpoint : DeleteCommandEndpoint<DeleteSampleEntityRequest, DeleteSampleEntityCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}
