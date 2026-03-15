using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/locations/{id:guid}", ApiMethod.Delete, ServiceName = "Location")]
public record DeleteLocationRequest([FromRoute] Guid Id) : ICrmRequest;
