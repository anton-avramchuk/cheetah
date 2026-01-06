using Cheetah.Core.CQRS;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Application.Queries;

/// <summary>
/// Query to get user by ID
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IQuery<UserViewModel?>;
