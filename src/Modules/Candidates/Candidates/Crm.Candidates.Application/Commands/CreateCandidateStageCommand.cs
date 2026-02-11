using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record CreateCandidateStageCommand(string Name, int Order, string? Color, bool IsDefault) : ICommand<Guid>;
