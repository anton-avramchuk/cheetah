using Cheetah.Core.CQRS;

namespace AppName.Application.Commands;

public record CreateSampleEntityCommand(string Name, string? Description) : ICommand<Guid>;
