using System.Security.Claims;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Exceptions;
using Cheetah.Modules.Identity.Application.Services;
using Shouldly;

namespace Cheetah.Modules.Identity.Application.Tests.Commands;

public class IssueServiceTokenCommandHandlerTests
{
    private sealed class StubAuthenticator(ServiceClientPrincipal? principal) : IServiceClientAuthenticator
    {
        public string? LastClientId { get; private set; }
        public string? LastSecret { get; private set; }

        public ServiceClientPrincipal? Authenticate(string clientId, string clientSecret)
        {
            LastClientId = clientId;
            LastSecret = clientSecret;
            return principal;
        }
    }

    private sealed class StubTokenGenerator : ITokenGenerator
    {
        public string? LastClientId { get; private set; }
        public IEnumerable<string>? LastRoles { get; private set; }

        public TokenResult GenerateToken(Guid userId, string userName, string email,
            IEnumerable<string> roles, IEnumerable<Claim> claims) =>
            throw new NotSupportedException();

        public TokenResult GenerateServiceToken(string clientId, IEnumerable<string> roles)
        {
            LastClientId = clientId;
            LastRoles = roles;
            return new TokenResult("svc.token", 600);
        }
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_IssuesServiceToken()
    {
        var authenticator = new StubAuthenticator(
            new ServiceClientPrincipal("svc-deals", new[] { "ServiceAccount" }));
        var generator = new StubTokenGenerator();
        var handler = new IssueServiceTokenCommandHandler(authenticator, generator);

        var result = await handler.HandleAsync(new IssueServiceTokenCommand("svc-deals", "secret"));

        result.Token.ShouldBe("svc.token");
        result.ExpiresInSeconds.ShouldBe(600);
        generator.LastClientId.ShouldBe("svc-deals");
        generator.LastRoles.ShouldContain("ServiceAccount");
    }

    [Fact]
    public async Task HandleAsync_InvalidCredentials_Throws()
    {
        var authenticator = new StubAuthenticator(principal: null);
        var handler = new IssueServiceTokenCommandHandler(authenticator, new StubTokenGenerator());

        await Should.ThrowAsync<InvalidCredentialsException>(() =>
            handler.HandleAsync(new IssueServiceTokenCommand("svc-deals", "wrong")).AsTask());
    }
}
