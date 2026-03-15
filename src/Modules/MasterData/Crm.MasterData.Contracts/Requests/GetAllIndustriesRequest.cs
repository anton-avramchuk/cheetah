using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/industries", ApiMethod.GetGrid, ResponseType = typeof(IndustryViewModel), ServiceName = "Industry")]
public class GetAllIndustriesRequest : GridRequest;
