using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/vacancy-tasks/{id:guid}/move", ApiMethod.Patch, ServiceName = "VacancyTasks")]
public record MoveVacancyTaskRequest([FromRoute] Guid Id, Guid StateId, int Order) : ICrmRequest;
