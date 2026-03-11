using Cheetah.Core.CQRS;

namespace Crm.Customer.Application.Commands;

public record CreateCustomerCommand(string Name, string? Description) : ICommand<Guid>;