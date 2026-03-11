using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints;

public class UpdateStackItemEndpoint : UpdateCommandEndpoint<UpdateStackItemRequest, UpdateStackItemCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}