using Cheetah.Core.CQRS;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Frontend.Client.Queries;

public record GetUserByEmailQuery(string Email) : IQuery<UserViewModel?>;
