using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// POST endpoint that accepts a file upload (multipart/form-data) and returns the command result.
///
/// Отличается от <see cref="CommandWithResultEndpoint{TRequest,TCommand,TCommandResult,TResponse}"/>
/// только источником запроса: он приезжает формой, а не телом JSON. Запрос поэтому объявляет
/// <c>IFormFile</c> (или <c>IFormFileCollection</c>) своим свойством, а перевод файла во что-то, о
/// чём знает прикладной слой, пишется в профиле маппинга — команде HTTP-типы не нужны.
///
/// Генератор ставит такому маршруту <c>DisableAntiforgery()</c>: метаданные формы иначе требуют
/// middleware защиты от подделки, и без него запрос отвечает 500 ещё до обработчика. Для загрузки
/// это осознанно — приложение, которому нужна защита от подделки на форме, включает
/// <c>UseAntiforgery()</c> и не пользуется этой базой.
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO (содержит IFormFile / IFormFileCollection)</typeparam>
/// <typeparam name="TCommand">CQRS Command</typeparam>
/// <typeparam name="TCommandResult">Command result</typeparam>
/// <typeparam name="TResponse">HTTP response DTO</typeparam>
public abstract class UploadCommandEndpoint<TRequest, TCommand, TCommandResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TCommand : ICommand<TCommandResult>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Post;
}
