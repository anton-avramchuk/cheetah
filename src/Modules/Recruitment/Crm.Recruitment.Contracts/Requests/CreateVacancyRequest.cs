using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancies", ApiMethod.Create, ServiceName = "Vacancies")]
public record CreateVacancyRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description,
    Guid? StateId = null,
    Guid? CustomerId = null,
    Guid? PositionId = null,
    Guid? StackItemId = null,
    Guid? WorkFormatId = null) : ICrmRequest;