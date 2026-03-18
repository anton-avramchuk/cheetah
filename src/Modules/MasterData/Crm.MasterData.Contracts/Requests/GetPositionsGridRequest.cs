using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/positions", ApiMethod.GetGrid, ResponseType = typeof(PositionViewModel), ServiceName = "Position")]
public class GetPositionsGridRequest : GridRequest;
