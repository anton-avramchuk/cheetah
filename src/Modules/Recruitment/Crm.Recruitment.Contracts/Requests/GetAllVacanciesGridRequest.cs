using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancies", ApiMethod.GetGrid, ResponseType = typeof(VacancyViewModel), ServiceName = "Vacancies")]
public class GetAllVacanciesGridRequest : GridRequest;
