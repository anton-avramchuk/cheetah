using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain.Entities;

namespace Cheetah.Modules.Activities.Application.Abstractions;

/// <summary>
/// Проекция конкретной активности в конкретный DTO (включая доп. поля наследника). Реализуется
/// наследником; используется generic query-handler'ами вместо Mapster, чтобы не требовать скрытой
/// конфигурации маппинга расширенных полей.
/// </summary>
public interface IActivityProjector<in TActivity, out TDto>
    where TActivity : ActivityBase
    where TDto : ActivityDtoBase
{
    TDto ToDto(TActivity activity);
}
