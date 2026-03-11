using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record CreateCandidateSourceCommand(string Name) : ICommand<Guid>;
