namespace Cheetah.Backend.IdentityCore.Api.Services.Abstractions;

public enum CrmSignInResult
{
    Success,
    InvalidCredentials,
    LockedOut,
    RequiresTwoFactor,
    NotAllowed
}