using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

public record GetVacancyTaskByIdRequest([FromRoute] Guid Id) : ICrmRequest;