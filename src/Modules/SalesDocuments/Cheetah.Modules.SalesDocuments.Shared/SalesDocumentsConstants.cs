namespace Cheetah.Modules.SalesDocuments.Shared;

/// <summary>
/// Общие константы шаблонного модуля SalesDocuments: имя БД-подключения, ограничения длин, дефолтные
/// имена таблиц/схемы и префикс маршрутов. Используются абстрактными базами
/// (<c>SalesDocumentConfigurationBase</c>, <c>SalesDocumentEndpointsBase</c>) как значения по
/// умолчанию, которые наследник может переопределить.
/// </summary>
public static class SalesDocumentsConstants
{
    public const string ConnectionStringName = "SalesDocuments";

    public const int MaxNumberLength = 64;
    public const int MaxNameLength = 300;
    public const int MaxCurrencyLength = 3;
    public const int MaxReasonLength = 1000;

    public const string DefaultSchema = "sales";
    public const string DefaultDocumentsTableName = "SalesDocuments";
    public const string DefaultLinesTableName = "SalesDocumentLines";

    public const string DefaultRoutePrefix = "api/sales-documents";
}
