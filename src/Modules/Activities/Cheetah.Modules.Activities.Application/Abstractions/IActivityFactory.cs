using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain.Entities;

namespace Cheetah.Modules.Activities.Application.Abstractions;

/// <summary>
/// Фабрика конкретной активности из запроса на создание. Реализуется наследником — он знает,
/// как сконструировать свою сущность (включая доп. поля) и завести инварианты/события через
/// <c>InitializeCore</c>. Так generic-handler создаёт активность, не зная конкретного типа.
/// </summary>
public interface IActivityFactory<out TActivity, in TCreateRequest>
    where TActivity : ActivityBase
    where TCreateRequest : CreateActivityRequestBase
{
    TActivity Create(TCreateRequest request);
}
