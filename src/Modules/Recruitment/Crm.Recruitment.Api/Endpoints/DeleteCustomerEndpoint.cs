using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class DeleteCustomerEndpoint : DeleteCommandEndpoint<DeleteCustomerRequest, DeleteCustomerCommand>
{
    public override string Route => $"{Constants.CustomerRoute}/{{id:guid}}";
}
