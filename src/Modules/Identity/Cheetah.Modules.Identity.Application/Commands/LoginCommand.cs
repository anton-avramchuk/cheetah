using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

public record LoginCommand(string UserName, string Password) : ICommand<TokenResult>;
