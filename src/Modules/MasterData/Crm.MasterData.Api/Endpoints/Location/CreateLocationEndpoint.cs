using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Location;

public class CreateLocationEndpoint : CreateCommandEndpoint<CreateLocationRequest, CreateLocationCommand>
{
    public override string Route => Constants.LocationsRoute;
    public override string GetByIdRouteName => "GetLocationById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Locations");
    }
}
