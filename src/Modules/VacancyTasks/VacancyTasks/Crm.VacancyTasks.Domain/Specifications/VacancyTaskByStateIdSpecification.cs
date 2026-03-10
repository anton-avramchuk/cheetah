using System.Linq.Expressions;
using Cheetah.Core.Specification;

namespace Crm.VacancyTasks.Domain.Specifications;

public class VacancyTaskByStateIdSpecification(Guid stateId) : Specification<VacancyTask>
{
    public override Expression<Func<VacancyTask, bool>> ToExpression() => t => t.StateId == stateId;
}
