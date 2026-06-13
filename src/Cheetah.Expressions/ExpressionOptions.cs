namespace Cheetah.Expressions;

/// <summary>
/// Лимиты безопасности для движка выражений.
/// Биндится из секции "Expressions" в конфигурации.
/// </summary>
public class ExpressionOptions
{
    /// <summary>
    /// Максимальная длина строки выражения в символах. По умолчанию 4096.
    /// </summary>
    public int MaxExpressionLength { get; set; } = 4096;

    /// <summary>
    /// Максимальная глубина вложенности операторов. По умолчанию 32.
    /// </summary>
    public int MaxNestingDepth { get; set; } = 32;

    /// <summary>
    /// Максимальное время выполнения одного вычисления. По умолчанию 1000 мс.
    /// Замеряется только время самого вычисления выражения (без учёта планирования
    /// пула потоков и JIT холодного старта), поэтому это «потолок» против патологически
    /// тяжёлых выражений, а не против разовой задержки первого вызова.
    /// </summary>
    public TimeSpan MaxEvaluationTime { get; set; } = TimeSpan.FromMilliseconds(1000);

    /// <summary>
    /// Тайм-аут regex-операторов. По умолчанию 50 мс.
    /// </summary>
    public TimeSpan RegexTimeout { get; set; } = TimeSpan.FromMilliseconds(50);
}
