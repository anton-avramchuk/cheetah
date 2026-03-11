using Cheetah.Core.CQRS;

namespace Crm.Customer.Application.Commands;

public record UpdateCustomerCommand(Guid Id, string Name, string? Description) : ICommand;