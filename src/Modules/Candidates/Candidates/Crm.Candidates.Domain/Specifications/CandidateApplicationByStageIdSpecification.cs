using System.Linq.Expressions;
using Cheetah.Core.Specification;

namespace Crm.Candidates.Domain.Specifications;

public class CandidateApplicationByStageIdSpecification(Guid stageId) : Specification<CandidateApplication>
{
    public override Expression<Func<CandidateApplication, bool>> ToExpression() => a => a.StageId == stageId;
}
