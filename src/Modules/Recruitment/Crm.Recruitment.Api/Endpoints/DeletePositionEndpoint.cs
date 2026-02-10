using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class DeletePositionEndpoint : DeleteCommandEndpoint<DeletePositionRequest, DeletePositionCommand>
{
    public override string Route => $"{Constants.PositionRoute}/{{id:guid}}";
}
