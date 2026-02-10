using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateCustomerEndpoint : CreateCommandEndpoint<CreateCustomerRequest, CreateCustomerCommand>
{
    public override string Route => Constants.CustomerRoute;

    public override string GetByIdRouteName => "GetCustomerById";
}
