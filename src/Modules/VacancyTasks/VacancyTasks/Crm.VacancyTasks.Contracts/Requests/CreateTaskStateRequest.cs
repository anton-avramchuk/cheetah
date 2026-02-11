using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-states", ApiMethod.Create, ServiceName = "TaskStates")]
public record CreateTaskStateRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color,
    bool IsDefault) : ICrmRequest;
