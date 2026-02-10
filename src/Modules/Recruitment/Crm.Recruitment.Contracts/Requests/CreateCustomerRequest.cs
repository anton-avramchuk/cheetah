using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

public record CreateCustomerRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description,
    Guid? DirectionId) : ICrmRequest;
