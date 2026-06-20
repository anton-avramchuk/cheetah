using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Domain.Specifications;

/// <summary>Флаг по ключу (уникальный контракт).</summary>
public sealed class FlagByKeySpecification<TFlag>(string key) : Specification<TFlag>
    where TFlag : FeatureFlagBase
{
    public override Expression<Func<TFlag, bool>> ToExpression() => f => f.Key == key;
}

/// <summary>Флаги, зарегистрированные конкретным сервисом.</summary>
public sealed class FlagsByOwnerServiceSpecification<TFlag>(string ownerService) : Specification<TFlag>
    where TFlag : FeatureFlagBase
{
    public override Expression<Func<TFlag, bool>> ToExpression() => f => f.OwnerService == ownerService;
}

/// <summary>Только активные флаги (для реплики/оценки).</summary>
public sealed class ActiveFlagsSpecification<TFlag>() : Specification<TFlag>
    where TFlag : FeatureFlagBase
{
    public override Expression<Func<TFlag, bool>> ToExpression() => f => f.IsActive;
}
