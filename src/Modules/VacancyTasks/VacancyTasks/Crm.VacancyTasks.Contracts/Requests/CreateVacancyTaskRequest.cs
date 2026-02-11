using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.VacancyTasks.Contracts.Requests;

public record CreateVacancyTaskRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;