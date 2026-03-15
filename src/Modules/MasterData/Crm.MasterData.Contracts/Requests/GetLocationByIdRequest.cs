using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/locations/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(LocationViewModel), ServiceName = "Location")]
public record GetLocationByIdRequest([FromRoute] Guid Id) : ICrmRequest;
