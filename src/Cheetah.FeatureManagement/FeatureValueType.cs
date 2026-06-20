namespace Cheetah.FeatureManagement;

/// <summary>
/// Тип значения флага. Живёт в абстракции (а не в Shared бизнес-модуля), т.к. на него опирается
/// сам движок оценки (<see cref="FeatureDefinition"/>), а бизнес-модуль зависит на абстракцию.
/// </summary>
public enum FeatureValueType
{
    /// <summary>Булев флаг: включён / выключен.</summary>
    Bool = 0,

    /// <summary>A/B/n: флаг отдаёт выбранный вариант (<see cref="FeatureVariant"/>).</summary>
    Variant = 1
}
