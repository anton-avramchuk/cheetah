using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/positions/{id:guid}", ApiMethod.Delete, ServiceName = "Position")]
public record DeletePositionRequest([FromRoute] Guid Id) : ICrmRequest;
