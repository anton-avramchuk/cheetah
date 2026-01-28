using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;

namespace Cheetah.Backend.Endpoints.Abstractions;

/// <summary>
/// Base interface for type-safe endpoint definitions with request/response
/// </summary>
/// <typeparam name="TRequest">Request type implementing ICrmRequest</typeparam>
/// <typeparam name="TResponse">Response type implementing ICrmResponse</typeparam>
public interface ICrmEndpoint<TRequest, TResponse> : IEndpointDefinition
    where TRequest : ICrmRequest
    where TResponse : ICrmResponse
{
    // Endpoint is fully declarative - no HandleAsync method needed
    // Behavior is determined by the CQRS command/query type
}
