using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/work-formats", ApiMethod.Create, ServiceName = "WorkFormat")]
public record CreateWorkFormatRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name) : ICrmRequest;
