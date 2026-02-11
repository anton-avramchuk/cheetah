namespace Crm.VacancyTasks.Application;

public record TaskStateModel(Guid Id, string Name, int Order, string? Color, bool IsDefault);
