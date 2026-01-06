using Cheetah.Core.CQRS;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Queries;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserViewModel?>;
