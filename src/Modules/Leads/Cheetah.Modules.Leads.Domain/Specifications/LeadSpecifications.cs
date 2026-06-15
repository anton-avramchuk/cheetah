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
                && l.Status != LeadStatus.Converted
                && l.Status != LeadStatus.Disqualified;
}

/// <summary>Комбинированный фильтр списка лидов (любой критерий опционален).</summary>
public sealed class LeadsFilterSpecification<TLead> : Specification<TLead>
    where TLead : LeadBase
{
    private readonly LeadStatus? _status;
    private readonly LeadSource? _source;
    private readonly Guid? _ownerId;

    public LeadsFilterSpecification(LeadStatus? status, LeadSource? source, Guid? ownerId)
    {
        _status = status;
        _source = source;
        _ownerId = ownerId;
    }

    public override Expression<Func<TLead, bool>> ToExpression()
        => l => (_status == null || l.Status == _status)
                && (_source == null || l.Source == _source)
                && (_ownerId == null || l.OwnerId == _ownerId);
}
