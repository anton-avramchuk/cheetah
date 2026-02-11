using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record DeleteCandidateStageCommand(Guid Id) : ICommand;
