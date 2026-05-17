namespace Cheetah.Notifications.Email;

/// <summary>
/// Вложение к notification-сообщению (например, к письму).
/// </summary>
public sealed record Attachment(string FileName, string ContentType, ReadOnlyMemory<byte> Content);
