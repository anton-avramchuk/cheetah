using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Deals.Domain.Entities;

namespace Cheetah.Modules.Deals.Domain.Specifications;

/// <summary>Активные воронки.</summary>
public sealed class ActivePipelinesSpecification : Specification<Pipeline>
{
    public override Expression<Func<Pipeline, bool>> ToExpression()
        => p => p.IsActive;
}

/// <summary>Воронка по умолчанию.</summary>
public sealed class DefaultPipelineSpecification : Specification<Pipeline>
{
    public override Expression<Func<Pipeline, bool>> ToExpression()
        => p => p.IsDefault;
}

/// <summary>История переходов сделки.</summary>
public sealed class StageHistoryByDealSpecification : Specification<DealStageHistory>
{
    private readonly Guid _dealId;
    public StageHistoryByDealSpecification(Guid dealId) => _dealId = dealId;
    public override Expression<Func<DealStageHistory, bool>> ToExpression()
        => h => h.DealId == _dealId;
}
