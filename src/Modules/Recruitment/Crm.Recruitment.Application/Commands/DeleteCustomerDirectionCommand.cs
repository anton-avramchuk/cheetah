using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record DeleteCustomerDirectionCommand(Guid Id) : ICommand;
