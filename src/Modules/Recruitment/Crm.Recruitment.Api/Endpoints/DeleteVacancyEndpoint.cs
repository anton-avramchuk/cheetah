using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class DeleteVacancyEndpoint : DeleteCommandEndpoint<DeleteVacancyRequest, DeleteVacancyCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";
}