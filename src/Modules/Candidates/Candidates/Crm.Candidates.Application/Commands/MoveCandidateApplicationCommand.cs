using Cheetah.Core.CQRS;

namespace Crm.Candidates.Application.Commands;

public record MoveCandidateApplicationCommand(Guid Id, Guid StageId, int Order) : ICommand;
