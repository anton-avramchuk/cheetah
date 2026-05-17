namespace Cheetah.FileStorage;

/// <summary>
/// Хелперы для безопасной работы с ключами хранилища.
/// </summary>
public static class StorageKey
{
    /// <summary>
    /// Проверяет, что ключ безопасен (нет path-traversal, недопустимых сегментов).
    /// Бросает <see cref="ArgumentException"/> если нет.
    /// Допустимы: буквы, цифры, '-', '_', '.', '/'. Сегменты "." и ".." запрещены.
    /// </summary>
    public static void Validate(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key is empty", nameof(key));
        if (key.StartsWith('/') || key.EndsWith('/'))
            throw new ArgumentException("Key must not start or end with '/'", nameof(key));
        if (key.Contains("//", StringComparison.Ordinal))
            throw new ArgumentException("Key must not contain empty segments ('//')", nameof(key));

        foreach (var segment in key.Split('/'))
        {
            if (segment is "." or "..")
                throw new ArgumentException($"Invalid segment '{segment}' in key '{key}'", nameof(key));
            foreach (var c in segment)
            {
                if (!(char.IsLetterOrDigit(c) || c is '-' or '_' or '.'))
                    throw new ArgumentException(
                        $"Invalid character '{c}' in key '{key}'. Allowed: letters, digits, '-', '_', '.', '/'.",
                        nameof(key));
            }
        }
    }
}
