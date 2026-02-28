using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/work-formats", ApiMethod.GetGrid, ResponseType = typeof(WorkFormatViewModel), ServiceName = "WorkFormats")]
public class GetAllWorkFormatsRequest : GridRequest;
