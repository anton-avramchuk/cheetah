using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record UpdateCandidateSourceCommand(Guid Id, string Name, int Order, string? Color) : ICommand;
