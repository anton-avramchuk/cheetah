namespace Cheetah.Backend.Jwt.Abstractions;

public record TokenGenerationResult(string Token, int ExpiresInSeconds);
