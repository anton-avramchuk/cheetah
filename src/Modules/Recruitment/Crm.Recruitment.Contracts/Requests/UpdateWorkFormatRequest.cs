using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/work-formats/{id:guid}", ApiMethod.Update, ServiceName = "WorkFormats")]
public record UpdateWorkFormatRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
