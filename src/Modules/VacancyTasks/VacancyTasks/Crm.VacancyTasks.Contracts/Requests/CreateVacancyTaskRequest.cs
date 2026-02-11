using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/vacancy-tasks", ApiMethod.Create, ServiceName = "VacancyTasks")]
public record CreateVacancyTaskRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Title,
    Guid VacancyId,
    Guid StateId,
    string? Description,
    Guid? PriorityId,
    Guid? AssigneeId,
    DateTimeOffset? DueDate) : ICrmRequest;
