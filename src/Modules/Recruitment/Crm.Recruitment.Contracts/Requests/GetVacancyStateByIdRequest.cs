using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;
namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/vacancy-states/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(VacancyStateViewModel), ServiceName = "VacancyStates")]
public record GetVacancyStateByIdRequest([FromRoute] Guid Id) : ICrmRequest;
