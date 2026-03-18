using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class LoginEndpoint : CommandWithResultEndpoint<LoginRequest, LoginCommand, TokenResult, TokenViewModel>
{
    public override string Route => "api/auth/login";

    protected override void Configure(EndpointConfiguration config)
    {
        config.AllowAnonymousAccess().WithTags("Auth");
    }
}
