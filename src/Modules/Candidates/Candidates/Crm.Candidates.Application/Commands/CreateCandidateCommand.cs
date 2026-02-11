using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record CreateCandidateCommand(string Name, string? Description) : ICommand<Guid>;