using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancy-states/{id:guid}", ApiMethod.Delete, ServiceName = "VacancyStates")]
public record DeleteVacancyStateRequest([FromRoute] Guid Id) : ICrmRequest;
