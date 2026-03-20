namespace AppName.Identity.Contracts.Response;

public record TokenViewModel(string AccessToken, string TokenType, int ExpiresIn)
    : Cheetah.Modules.Identity.Contracts.Response.TokenViewModel(AccessToken, TokenType, ExpiresIn);
