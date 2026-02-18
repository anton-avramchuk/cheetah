using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancy-states", ApiMethod.Create, ServiceName = "VacancyStates")]
public record CreateVacancyStateRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color,
    bool IsDefault) : ICrmRequest;
