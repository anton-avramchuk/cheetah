using Cheetah.Core.CQRS;

namespace Crm.Customer.Application.Commands;

public record DeleteCustomerCommand(Guid Id) : ICommand;