using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateWorkFormatEndpoint : CreateCommandEndpoint<CreateWorkFormatRequest, CreateWorkFormatCommand>
{
    public override string Route => Constants.WorkFormatRoute;

    public override string GetByIdRouteName => "GetWorkFormatById";
}
