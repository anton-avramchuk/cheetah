using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class UpdateStackItemEndpoint : UpdateCommandEndpoint<UpdateStackItemRequest, UpdateStackItemCommand>
{
    public override string Route => $"{Constants.StackItemRoute}/{{id:guid}}";
}
