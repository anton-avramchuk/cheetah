using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record CreateCandidateSourceCommand(string Name, int Order, string? Color) : ICommand<Guid>;
