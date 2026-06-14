namespace Cheetah.Modules.Email.Shared;

/// <summary>Константы модуля Email: имя БД-подключения и ограничения длин.</summary>
public static class EmailConstants
{
    public const string ConnectionStringName = "Email";

    public const int MaxAddressLength = 256;
    public const int MaxProviderMessageIdLength = 256;
    public const int MaxErrorLength = 1024;
}
