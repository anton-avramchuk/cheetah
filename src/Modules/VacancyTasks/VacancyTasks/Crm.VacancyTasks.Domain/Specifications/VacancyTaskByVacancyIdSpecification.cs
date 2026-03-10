using System.Linq.Expressions;
using Cheetah.Core.Specification;

namespace Crm.VacancyTasks.Domain.Specifications;

public class VacancyTaskByVacancyIdSpecification(Guid vacancyId) : Specification<VacancyTask>
{
    public override Expression<Func<VacancyTask, bool>> ToExpression() => t => t.VacancyId == vacancyId;
}
