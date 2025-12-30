using Cheetah.Core.CQRS;
using Cheetah.Identity.Shared.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Queries;

public record GetUserByEmailQuery(string Email) : IQuery<UserViewModel?>;
