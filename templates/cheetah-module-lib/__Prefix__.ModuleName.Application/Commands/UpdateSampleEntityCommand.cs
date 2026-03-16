using Cheetah.Core.CQRS;

namespace __Prefix__.ModuleName.Application.Commands;

public record UpdateSampleEntityCommand(Guid Id, string Name, string? Description) : ICommand;
