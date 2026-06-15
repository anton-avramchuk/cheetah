using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;

namespace Cheetah.Modules.Leads.Application.Abstractions;

/// <summary>
/// Проекция конкретного лида в конкретный DTO (включая доп. поля наследника и VO → строки).
/// Реализуется наследником; используется generic query-handler'ами вместо Mapster.
/// </summary>
public interface ILeadProjector<in TLead, out TDto>
    where TLead : LeadBase
    where TDto : LeadDtoBase
{
    TDto ToDto(TLead lead);
}
