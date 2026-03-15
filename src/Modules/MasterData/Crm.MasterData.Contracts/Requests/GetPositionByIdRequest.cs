using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/positions/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(PositionViewModel), ServiceName = "Position")]
public record GetPositionByIdRequest([FromRoute] Guid Id) : ICrmRequest;
