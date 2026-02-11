using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/work-formats", ApiMethod.GetCollection, ResponseType = typeof(WorkFormatViewModel), ServiceName = "WorkFormats")]
public record GetAllWorkFormatsRequest : ICrmRequest;
