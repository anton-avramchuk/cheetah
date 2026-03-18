using Cheetah.Core.CQRS;

namespace AppName.Application.Commands;

public record UpdateSampleEntityCommand(Guid Id, string Name, string? Description) : ICommand;
