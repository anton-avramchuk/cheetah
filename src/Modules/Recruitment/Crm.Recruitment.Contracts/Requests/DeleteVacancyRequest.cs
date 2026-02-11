using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancies/{id:guid}", ApiMethod.Delete, ServiceName = "Vacancies")]
public record DeleteVacancyRequest([FromRoute] Guid Id) : ICrmRequest;