using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Commands;

namespace AppName.Identity.Api.Endpoints;

public sealed class LoginEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.LoginEndpoint<LoginRequest, LoginCommand>
{
}
