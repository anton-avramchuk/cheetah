using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancy-states/{id:guid}", ApiMethod.Update, ServiceName = "VacancyStates")]
public record UpdateVacancyStateRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color,
    bool IsDefault) : ICrmRequest;
