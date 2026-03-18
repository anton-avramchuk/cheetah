using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class LoginEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.LoginEndpoint<LoginRequest, LoginCommand>
{
}
