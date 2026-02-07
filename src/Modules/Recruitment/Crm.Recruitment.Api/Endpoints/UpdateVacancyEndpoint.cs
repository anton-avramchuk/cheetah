using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class UpdateVacancyEndpoint : UpdateCommandEndpoint<UpdateVacancyRequest, UpdateVacancyCommand>
{
    public override string Route => $"{Constants.VacancyRoute}/{{id:guid}}";
}