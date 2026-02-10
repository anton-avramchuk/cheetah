using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdateCustomerCommand(Guid Id, string Name, string? Description, Guid? DirectionId) : ICommand;
