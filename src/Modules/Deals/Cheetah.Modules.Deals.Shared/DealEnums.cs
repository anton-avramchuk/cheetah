namespace Cheetah.Modules.Deals.Shared;

/// <summary>Статус сделки. Open — в работе; Won/Lost — терминальные.</summary>
public enum DealStatus
{
    /// <summary>В работе (движется по стадиям воронки).</summary>
    Open = 0,
    /// <summary>Выиграна.</summary>
    Won = 1,
    /// <summary>Проиграна.</summary>
    Lost = 2
}

/// <summary>Тип стадии воронки — определяет, к какому статусу приводит попадание на стадию.</summary>
public enum StageType
{
    /// <summary>Промежуточная стадия открытой сделки.</summary>
    Open = 0,
    /// <summary>Стадия выигрыша (терминальная).</summary>
    Won = 1,
    /// <summary>Стадия проигрыша (терминальная).</summary>
    Lost = 2
}

/// <summary>Источник изменения сделки (для аудита/автоматизаций).</summary>
public enum TriggerSource
{
    /// <summary>Изменение вручную пользователем.</summary>
    Manual = 0,
    /// <summary>Изменение автоматизацией (Workflow).</summary>
    Automation = 1,
    /// <summary>Изменение при импорте.</summary>
    Import = 2
}
