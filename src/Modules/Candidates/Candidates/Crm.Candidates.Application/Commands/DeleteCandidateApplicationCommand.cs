using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record DeleteCandidateApplicationCommand(Guid Id) : ICommand;
