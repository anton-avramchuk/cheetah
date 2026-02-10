using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class UpdateWorkFormatEndpoint : UpdateCommandEndpoint<UpdateWorkFormatRequest, UpdateWorkFormatCommand>
{
    public override string Route => $"{Constants.WorkFormatRoute}/{{id:guid}}";
}
