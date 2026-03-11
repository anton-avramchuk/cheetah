using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Location;

public class UpdateLocationEndpoint : UpdateCommandEndpoint<UpdateLocationRequest, UpdateLocationCommand>
{
    public override string Route => $"{Constants.LocationsRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Locations");
    }
}
