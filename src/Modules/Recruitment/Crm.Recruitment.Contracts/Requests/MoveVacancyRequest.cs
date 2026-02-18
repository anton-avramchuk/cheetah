using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancies/{id:guid}/move", ApiMethod.Patch, ServiceName = "Vacancies")]
public record MoveVacancyRequest([FromRoute] Guid Id, Guid StateId, int Order) : ICrmRequest;
