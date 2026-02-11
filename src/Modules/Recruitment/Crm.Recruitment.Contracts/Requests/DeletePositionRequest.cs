using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/positions/{id:guid}", ApiMethod.Delete, ServiceName = "Positions")]
public record DeletePositionRequest([FromRoute] Guid Id) : ICrmRequest;
