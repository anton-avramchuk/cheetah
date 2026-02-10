using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreateCustomerCommand(string Name, string? Description, Guid? DirectionId) : ICommand<Guid>;
