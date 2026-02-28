using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancy-states", ApiMethod.GetGrid, ResponseType = typeof(VacancyStateViewModel), ServiceName = "VacancyStates")]
public class GetAllVacancyStatesRequest : GridRequest;
