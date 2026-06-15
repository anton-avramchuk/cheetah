using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;

namespace Cheetah.Modules.Leads.Application.Abstractions;

/// <summary>
/// Фабрика конкретного лида из запроса на создание. Реализуется наследником — он знает, как
/// сконструировать свою сущность (включая доп. поля) через <c>InitializeCore</c>. Так generic-handler
/// создаёт лид, не зная конкретного типа.
/// </summary>
public interface ILeadFactory<out TLead, in TCreateRequest>
    where TLead : LeadBase
    where TCreateRequest : CreateLeadRequestBase
{
    TLead Create(TCreateRequest request);
}
