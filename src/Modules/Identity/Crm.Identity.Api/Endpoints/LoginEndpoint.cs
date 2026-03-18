using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class LoginEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.LoginEndpoint<LoginRequest, LoginCommand>
{
}
