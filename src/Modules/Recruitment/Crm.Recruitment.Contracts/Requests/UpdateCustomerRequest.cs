using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customers/{id:guid}", ApiMethod.Update, ServiceName = "Customers")]
public record UpdateCustomerRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Code,
    string? Description,
    Guid? DirectionId) : ICrmRequest;
