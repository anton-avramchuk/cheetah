using System.Linq.Expressions;
using Cheetah.Core.Specification;

namespace Crm.Recruitment.Domain.Specifications;

public class VacancyByStateIdSpecification(Guid stateId) : Specification<Vacancy>
{
    public override Expression<Func<Vacancy, bool>> ToExpression() => v => v.StateId == stateId;
}
