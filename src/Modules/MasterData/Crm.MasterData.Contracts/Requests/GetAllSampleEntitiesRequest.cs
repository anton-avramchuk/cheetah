using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/stack-items", ApiMethod.GetGrid, ResponseType = typeof(StackItemViewModel), ServiceName = "StackItem")]
public class GetAllSampleEntitiesRequest : GridRequest;
