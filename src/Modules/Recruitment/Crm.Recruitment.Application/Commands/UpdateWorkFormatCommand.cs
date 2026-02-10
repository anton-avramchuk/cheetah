using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdateWorkFormatCommand(Guid Id, string Name) : ICommand;
