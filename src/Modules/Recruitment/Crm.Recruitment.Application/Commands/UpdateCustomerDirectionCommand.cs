using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdateCustomerDirectionCommand(Guid Id, string Name, string? Description) : ICommand;
