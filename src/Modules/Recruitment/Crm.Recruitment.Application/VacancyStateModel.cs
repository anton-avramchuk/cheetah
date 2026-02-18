namespace Crm.Recruitment.Application;

public record VacancyStateModel(Guid Id, string Name, int Order, string? Color, bool IsDefault);
