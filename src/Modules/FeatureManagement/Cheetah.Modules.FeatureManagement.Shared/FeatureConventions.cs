namespace Cheetah.Modules.FeatureManagement.Shared;

/// <summary>Характер раскатки флага (для UI/аналитики; движок работает над правилами таргетинга).</summary>
public enum RolloutType
{
    /// <summary>Выключен глобально (kill-switch off).</summary>
    Off = 0,

    /// <summary>Включён для всех (нет ограничивающих правил).</summary>
    On = 1,

    /// <summary>Процентная раскатка.</summary>
    Percentage = 2,

    /// <summary>Адресный таргетинг (списки/роли/условия).</summary>
    Targeted = 3
}

/// <summary>Конвенции ключей флагов: стабильный публичный контракт <c>"{service}.{feature}"</c>.</summary>
public static class FeatureKeys
{
    public static string Compose(string service, string feature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(feature);
        return $"{service.Trim()}.{feature.Trim()}";
    }
}
