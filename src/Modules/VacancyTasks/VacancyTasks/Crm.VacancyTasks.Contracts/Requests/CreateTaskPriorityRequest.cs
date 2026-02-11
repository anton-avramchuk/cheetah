using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/task-priorities", ApiMethod.Create, ServiceName = "TaskPriorities")]
public record CreateTaskPriorityRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    int Order,
    string? Color) : ICrmRequest;
