using Cheetah.AspNetCore.Contracts.Responses;

namespace Cheetah.Backend.Endpoints.Responses;

/// <summary>
/// Represents an endpoint that doesn't return data (NoContent, etc.)
/// </summary>
public sealed record EmptyResponse : ICrmResponse;
