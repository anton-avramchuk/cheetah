using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/vacancy-tasks/{id:guid}", ApiMethod.Delete, ServiceName = "VacancyTasks")]
public record DeleteVacancyTaskRequest([FromRoute] Guid Id) : ICrmRequest;