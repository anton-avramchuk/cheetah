namespace Cheetah.Backend.IdentityCore.Api.Services.Abstractions;

public interface IAuthService
{
    Task<CrmSignInResult> SignIn(string userName, string password);
}