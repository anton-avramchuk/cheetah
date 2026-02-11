using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/positions/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(PositionViewModel), ServiceName = "Positions")]
public record GetPositionByIdRequest([FromRoute] Guid Id) : ICrmRequest;
