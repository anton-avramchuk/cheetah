using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Contracts.Requests;

[ApiRoute("api/vacancy-tasks/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(VacancyTaskViewModel))]
public record GetVacancyTaskByIdRequest([FromRoute] Guid Id) : ICrmRequest;