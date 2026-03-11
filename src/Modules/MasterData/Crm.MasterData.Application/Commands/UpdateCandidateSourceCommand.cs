using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record UpdateCandidateSourceCommand(Guid Id, string Name) : ICommand;
