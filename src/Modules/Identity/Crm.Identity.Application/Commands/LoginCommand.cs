using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record LoginCommand(string UserName, string Password) : ICommand<TokenResult>;
