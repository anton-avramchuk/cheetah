using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Domain.Specifications;

/// <summary>Открытые сделки указанного ответственного.</summary>
public sealed class OpenDealsByOwnerSpecification : Specification<Deal>
{
    private readonly Guid _ownerId;
    public OpenDealsByOwnerSpecification(Guid ownerId) => _ownerId = ownerId;
    public override Expression<Func<Deal, bool>> ToExpression()
        => d => d.OwnerId == _ownerId && d.Status == DealStatus.Open;
}

/// <summary>Сделки воронки (опционально — конкретной стадии).</summary>
public sealed class DealsByPipelineStageSpecification : Specification<Deal>
{
    private readonly Guid _pipelineId;
    private readonly Guid? _stageId;
    public DealsByPipelineStageSpecification(Guid pipelineId, Guid? stageId = null)
        => (_pipelineId, _stageId) = (pipelineId, stageId);
    public override Expression<Func<Deal, bool>> ToExpression()
        => d => d.PipelineId == _pipelineId && (_stageId == null || d.StageId == _stageId);
}

/// <summary>Сделки клиента.</summary>
public sealed class DealsByCustomerSpecification : Specification<Deal>
{
    private readonly Guid _customerId;
    public DealsByCustomerSpecification(Guid customerId) => _customerId = customerId;
    public override Expression<Func<Deal, bool>> ToExpression()
        => d => d.CustomerId == _customerId;
}

/// <summary>
/// Комбинируемый фильтр списка сделок: каждый заданный параметр сужает выборку.
/// Несколько простых условий собраны в одно выражение, чтобы провайдер транслировал их в SQL.
/// </summary>
public sealed class DealsFilterSpecification : Specification<Deal>
{
    private readonly Guid? _ownerId;
    private readonly Guid? _pipelineId;
    private readonly Guid? _stageId;
    private readonly Guid? _customerId;
    private readonly DealStatus? _status;

    public DealsFilterSpecification(
        Guid? ownerId = null, Guid? pipelineId = null, Guid? stageId = null,
        Guid? customerId = null, DealStatus? status = null)
    {
        _ownerId = ownerId;
        _pipelineId = pipelineId;
        _stageId = stageId;
        _customerId = customerId;
        _status = status;
    }

    public override Expression<Func<Deal, bool>> ToExpression()
        => d => (_ownerId == null || d.OwnerId == _ownerId)
                && (_pipelineId == null || d.PipelineId == _pipelineId)
                && (_stageId == null || d.StageId == _stageId)
                && (_customerId == null || d.CustomerId == _customerId)
                && (_status == null || d.Status == _status);
}
