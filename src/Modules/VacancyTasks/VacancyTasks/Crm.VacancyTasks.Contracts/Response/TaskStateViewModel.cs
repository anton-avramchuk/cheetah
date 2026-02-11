using Cheetah.Contracts.Responses;

namespace Crm.VacancyTasks.Contracts.Response;

public record TaskStateViewModel(Guid Id, string Name, int Order, string? Color, bool IsDefault) : ICrmResponse;
