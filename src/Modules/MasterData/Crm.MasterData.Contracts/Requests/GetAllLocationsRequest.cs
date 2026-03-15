using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/locations", ApiMethod.GetGrid, ResponseType = typeof(LocationViewModel), ServiceName = "Location")]
public class GetAllLocationsRequest : GridRequest;
