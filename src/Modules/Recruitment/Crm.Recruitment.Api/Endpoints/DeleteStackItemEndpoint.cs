using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class DeleteStackItemEndpoint : DeleteCommandEndpoint<DeleteStackItemRequest, DeleteStackItemCommand>
{
    public override string Route => $"{Constants.StackItemRoute}/{{id:guid}}";
}
