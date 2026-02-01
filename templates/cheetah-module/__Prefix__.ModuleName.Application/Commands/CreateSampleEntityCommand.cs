using Cheetah.Core.CQRS;

namespace __Prefix__.ModuleName.Application.Commands;

public record CreateSampleEntityCommand(string Name, string? Description) : ICommand<Guid>;
