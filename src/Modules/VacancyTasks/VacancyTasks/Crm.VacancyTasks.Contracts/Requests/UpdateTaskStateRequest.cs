using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-states/{id:guid}", ApiMethod.Update, ServiceName = "TaskStates")]
public record UpdateTaskStateRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color,
    bool IsDefault) : ICrmRequest;
