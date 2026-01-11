using Cheetah.AspNetCore.Contracts.Responses;

namespace Cheetah.Backend.Endpoints.Responses;

/// <summary>
/// Represents an endpoint that returns a GUID (typically created resource ID)
/// </summary>
public sealed record GuidResponse(Guid Id) : ICrmResponse;
