namespace Cheetah.Modules.Tags.Shared;

/// <summary>
/// Общие константы модуля тэгов: имя БД-подключения, ограничения длин,
/// конвенция ключей применимых типов сущностей (<c>"{service}.{entity}"</c>).
/// </summary>
public static class TagsConstants
{
    public const string ConnectionStringName = "Tags";

    public const int MaxNameLength = 128;
    public const int MaxEntityTypeKeyLength = 128;
    public const int MaxColorLength = 32;
    public const int MaxGroupLength = 64;
}
