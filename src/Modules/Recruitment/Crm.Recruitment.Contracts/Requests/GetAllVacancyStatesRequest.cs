using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancy-states", ApiMethod.GetCollection, ResponseType = typeof(VacancyStateViewModel), ServiceName = "VacancyStates")]
public record GetAllVacancyStatesRequest : ICrmRequest;
