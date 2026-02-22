using Cheetah.Contracts.Responses;

namespace Crm.Identity.Contracts.Response;

public record TokenViewModel(
    string AccessToken,
    string TokenType,
    int ExpiresIn) : ICrmResponse;
