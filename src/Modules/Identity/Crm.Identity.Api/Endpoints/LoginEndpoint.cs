using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Api.Endpoints;

public class LoginEndpoint : CommandWithResultEndpoint<LoginRequest, LoginCommand, TokenResult, TokenViewModel>
{
    public override string Route => "api/auth/login";

    protected override void Configure(EndpointConfiguration config)
    {
        config.AllowAnonymousAccess().WithTags("Auth");
    }
}
