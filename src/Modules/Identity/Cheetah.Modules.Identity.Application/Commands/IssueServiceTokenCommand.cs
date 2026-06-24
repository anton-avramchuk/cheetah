using Cheetah.Core.CQRS;

namespace Cheetah.Modules.Identity.Application.Commands;

/// <summary>
/// Выпуск сервисного токена по схеме client_credentials (machine-to-machine).
/// </summary>
public record IssueServiceTokenCommand(string ClientId, string ClientSecret) : ICommand<TokenResult>;
