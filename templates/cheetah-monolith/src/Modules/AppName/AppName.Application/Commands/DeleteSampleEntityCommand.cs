using Cheetah.Core.CQRS;

namespace AppName.Application.Commands;

public record DeleteSampleEntityCommand(Guid Id) : ICommand;
