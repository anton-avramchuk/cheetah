using Cheetah.Core.CQRS;

namespace __Prefix__.ModuleName.Application.Commands;

public record DeleteSampleEntityCommand(Guid Id) : ICommand;
