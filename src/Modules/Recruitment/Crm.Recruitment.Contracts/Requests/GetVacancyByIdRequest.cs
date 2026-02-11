using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancies/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(VacancyViewModel), ServiceName = "Vacancies")]
public record GetVacancyByIdRequest([FromRoute] Guid Id) : ICrmRequest;