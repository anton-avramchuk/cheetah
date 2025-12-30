using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Frontend.Client.Queries;

public record GetUserPermissionsQuery(Guid UserId) : IQuery<List<string>>;
