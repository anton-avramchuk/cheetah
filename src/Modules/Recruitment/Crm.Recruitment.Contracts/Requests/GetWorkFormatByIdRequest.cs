using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/work-formats/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(WorkFormatViewModel), ServiceName = "WorkFormats")]
public record GetWorkFormatByIdRequest([FromRoute] Guid Id) : ICrmRequest;
