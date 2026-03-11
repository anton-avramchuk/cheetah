using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Position;

public class CreatePositionEndpoint : CreateCommandEndpoint<CreatePositionRequest, CreatePositionCommand>
{
    public override string Route => Constants.PositionsRoute;
    public override string GetByIdRouteName => "GetPositionById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Positions");
    }
}
