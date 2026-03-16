namespace Cheetah.Modules.Identity.Application.Commands;

public record TokenResult(string Token, int ExpiresInSeconds);
