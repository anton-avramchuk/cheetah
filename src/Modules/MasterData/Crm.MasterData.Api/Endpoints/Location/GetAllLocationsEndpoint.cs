using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.Location;

public class GetAllLocationsEndpoint : QueryGridEndpoint<GetAllLocationsRequest,
    GetAllLocationsQuery, LocationModel, LocationViewModel>
{
    public override string Route => Constants.LocationsRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Locations");
    }
}
