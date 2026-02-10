using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

public record CreateCustomerDirectionRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;
