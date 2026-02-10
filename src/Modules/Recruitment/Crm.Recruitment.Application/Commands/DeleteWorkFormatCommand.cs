using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record DeleteWorkFormatCommand(Guid Id) : ICommand;
