using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Queries;

public record GetAllSampleEntitiesQuery(
    int Page,
    int PageSize,
    List<SortDescriptor> Sort,
    FilterDescriptor? Filter
) : IQuery<GridResult<VacancyTaskModel>>;