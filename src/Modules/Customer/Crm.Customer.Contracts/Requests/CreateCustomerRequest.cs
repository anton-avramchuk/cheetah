using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.Customer.Contracts.Requests;

public record CreateCustomerRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description,
    Guid? IndustryId = null) : ICrmRequest;