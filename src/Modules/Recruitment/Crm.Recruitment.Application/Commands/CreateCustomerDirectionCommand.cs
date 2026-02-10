using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreateCustomerDirectionCommand(string Name, string? Description) : ICommand<Guid>;
