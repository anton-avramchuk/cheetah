using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record UpdateCandidateStageCommand(Guid Id, string Name, int Order, string? Color, bool IsDefault) : ICommand;
