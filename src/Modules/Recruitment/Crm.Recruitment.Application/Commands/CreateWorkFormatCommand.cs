using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreateWorkFormatCommand(string Name) : ICommand<Guid>;
