using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record DeleteCandidateSourceCommand(Guid Id) : ICommand;
