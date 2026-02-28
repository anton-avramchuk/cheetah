using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/positions", ApiMethod.GetGrid, ResponseType = typeof(PositionViewModel), ServiceName = "Positions")]
public class GetAllPositionsRequest : GridRequest;
