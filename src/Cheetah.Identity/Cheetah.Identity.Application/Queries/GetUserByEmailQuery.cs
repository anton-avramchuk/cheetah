using Cheetah.Core.CQRS;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Application.Queries;

/// <summary>
/// Query to get user by email
/// </summary>
public record GetUserByEmailQuery(string Email) : IQuery<UserViewModel?>;
