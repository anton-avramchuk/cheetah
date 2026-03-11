using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.StackItem;

public class CreateStackItemEndpoint : CreateCommandEndpoint<CreateStackItemRequest, CreateStackItemCommand>
{
    public override string Route => Constants.DefaultRoute;

    public override string GetByIdRouteName => "GetStackItemById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}