using System.Linq.Expressions;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Core.Specification;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Domain.Specifications;

/// <summary>Лид с указанным email (антидубль; сравнение по value-converted колонке).</summary>
public sealed class LeadByEmailSpecification<TLead> : Specification<TLead>
    where TLead : LeadBase
{
    private readonly Email _email;
    public LeadByEmailSpecification(string email) => _email = Email.Create(email);

    public override Expression<Func<TLead, bool>> ToExpression()
        => l => l.Email == _email;
}

/// <summary>Лид с указанным телефоном (антидубль).</summary>
public sealed class LeadByPhoneSpecification<TLead> : Specification<TLead>
    where TLead : LeadBase
{
    private readonly Phone _phone;
    public LeadByPhoneSpecification(string phone) => _phone = Phone.Create(phone);

    public override Expression<Func<TLead, bool>> ToExpression()
        => l => l.Phone == _phone;
}

/// <summary>Активные лиды (не Converted и не Disqualified) указанного ответственного.</summary>
public sealed class ActiveLeadsByOwnerSpecification<TLead> : Specification<TLead>
    where TLead : LeadBase
{
    private readonly Guid _ownerId;
    public ActiveLeadsByOwnerSpecification(Guid ownerId) => _ownerId = ownerId;

    public override Expression<Func<TLead, bool>> ToExpression()
        => l => l.OwnerId == _ownerId
                && l.StatusId != LeadWellKnownIds.StatusConverted
                && l.StatusId != LeadWellKnownIds.StatusDisqualified;
}

/// <summary>Комбинированный фильтр списка лидов (любой критерий опционален).</summary>
public sealed class LeadsFilterSpecification<TLead> : Specification<TLead>
    where TLead : LeadBase
{
    private readonly Guid? _statusId;
    private readonly Guid? _sourceId;
    private readonly Guid? _ownerId;

    public LeadsFilterSpecification(Guid? statusId, Guid? sourceId, Guid? ownerId)
    {
        _statusId = statusId;
        _sourceId = sourceId;
        _ownerId = ownerId;
    }

    public override Expression<Func<TLead, bool>> ToExpression()
        => l => (_statusId == null || l.StatusId == _statusId)
                && (_sourceId == null || l.SourceId == _sourceId)
                && (_ownerId == null || l.OwnerId == _ownerId);
}
