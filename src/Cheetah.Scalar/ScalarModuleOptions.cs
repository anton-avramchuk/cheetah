namespace Cheetah.Scalar;

public class ScalarModuleOptions
{
    /// <summary>
    /// Путь к OpenAPI-документу для UI. Пусто — берётся из имени документа
    /// (<c>/openapi/{documentName}.json</c>).
    /// </summary>
    public string? OpenApiPath { get; set; }
}
