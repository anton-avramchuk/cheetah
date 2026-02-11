namespace Crm.VacancyTasks.Application;

public record TaskPriorityModel(Guid Id, string Name, int Order, string? Color);
