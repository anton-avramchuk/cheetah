using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/work-formats", ApiMethod.GetGrid, ResponseType = typeof(WorkFormatViewModel), ServiceName = "WorkFormat")]
public class GetWorkFormatsGridRequest : GridRequest;
