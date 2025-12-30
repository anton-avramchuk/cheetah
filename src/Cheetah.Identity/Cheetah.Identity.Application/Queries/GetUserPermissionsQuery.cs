using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Queries;

/// <summary>
/// Query to get all permissions for a user
/// Combines permissions from assigned roles + personal user claims
/// </summary>
public record GetUserPermissionsQuery(Guid UserId) : IQuery<List<string>>;
