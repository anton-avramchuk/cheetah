using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record UpdateCandidateCommand(Guid Id, string Name, string? Description) : ICommand;