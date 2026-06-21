namespace Cheetah.Workflow;

/// <summary>
/// Дескриптор триггера — модуль-источник декларирует, какое событие он публикует (для построения правил
/// в админке). Наполняет каталог через <c>registry/sync</c>, как Permissions.Catalog/Tags/FeatureManagement.
/// </summary>
public sealed record TriggerDescriptor(
    string EventName,
    string OwnerService,
    string Title,
    IReadOnlyList<string> PayloadFields);

/// <summary>
/// Дескриптор действия — модуль-цель декларирует, какую операцию он открывает Workflow, её параметры и
/// транспорт, которым её исполнять (<see cref="ActionTransport"/>).
/// </summary>
public sealed record ActionDescriptor(
    string Name,
    string OwnerService,
    string Title,
    IReadOnlyList<ActionParameterDescriptor> Parameters,
    ActionTransport Transport);

/// <summary>Описание одного параметра действия (для валидации и подсказок в UI).</summary>
public sealed record ActionParameterDescriptor(string Key, string Type, bool Required, string? Default = null);

/// <summary>Транспорт исполнения действия: in-proc плагин, команда-событие по шине или шаблонный HTTP/gRPC.</summary>
public enum ActionTransport
{
    InProc = 0,
    Bus = 1,
    Http = 2
}
