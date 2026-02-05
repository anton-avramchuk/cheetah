namespace Crm.Recruitment.Application;

public record VacancyRoleModel(
    Guid Id,
    string Name,
    string Code,
    bool IsSingle,
    int Order);
