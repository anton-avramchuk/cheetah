namespace Cheetah.Core.Outbox;

/// <summary>
/// Маркер для IEventHandler: при регистрации хендлер автоматически оборачивается
/// в InboxIdempotentEventHandler. Регистрацию выполняет Source Generator
/// (Cheetah.Generators.Application) либо ручной вызов services.AddIdempotentHandler.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IdempotentAttribute : Attribute
{
}
