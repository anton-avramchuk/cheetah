using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record DeleteCustomerCommand(Guid Id) : ICommand;
