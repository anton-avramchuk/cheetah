using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customers", ApiMethod.Create, ServiceName = "Customers")]
public record CreateCustomerRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description,
    Guid? DirectionId) : ICrmRequest;
