using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class DeleteCustomerDirectionEndpoint : DeleteCommandEndpoint<DeleteCustomerDirectionRequest, DeleteCustomerDirectionCommand>
{
    public override string Route => $"{Constants.CustomerDirectionRoute}/{{id:guid}}";
}
