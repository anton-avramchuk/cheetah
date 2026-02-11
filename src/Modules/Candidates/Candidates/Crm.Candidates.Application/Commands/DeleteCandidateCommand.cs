using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record DeleteCandidateCommand(Guid Id) : ICommand;