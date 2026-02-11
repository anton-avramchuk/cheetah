using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/work-formats", ApiMethod.Create, ServiceName = "WorkFormats")]
public record CreateWorkFormatRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
