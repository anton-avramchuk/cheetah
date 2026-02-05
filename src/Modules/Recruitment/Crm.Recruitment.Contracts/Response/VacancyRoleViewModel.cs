using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record VacancyRoleViewModel(
    Guid Id,
    string Name,
    string Code,
    bool IsSingle,
    int Order) : ICrmResponse;
