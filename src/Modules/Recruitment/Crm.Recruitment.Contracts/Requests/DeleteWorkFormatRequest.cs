using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/work-formats/{id:guid}", ApiMethod.Delete, ServiceName = "WorkFormats")]
public record DeleteWorkFormatRequest([FromRoute] Guid Id) : ICrmRequest;
