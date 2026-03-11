using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Position;

public class UpdatePositionEndpoint : UpdateCommandEndpoint<UpdatePositionRequest, UpdatePositionCommand>
{
    public override string Route => $"{Constants.PositionsRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Positions");
    }
}
