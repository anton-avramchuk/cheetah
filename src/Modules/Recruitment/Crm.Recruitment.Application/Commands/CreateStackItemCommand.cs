using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreateStackItemCommand(string Name) : ICommand<Guid>;
