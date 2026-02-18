using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/vacancy-tasks/{id:guid}", ApiMethod.Update, ServiceName = "VacancyTasks")]
public record UpdateVacancyTaskRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Title,
    string? Description,
    Guid StateId,
    Guid? PriorityId,
    DateTimeOffset? DueDate) : ICrmRequest;
