using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/positions", ApiMethod.GetCollection, ResponseType = typeof(PositionViewModel), ServiceName = "Positions")]
public record GetAllPositionsRequest : ICrmRequest;
