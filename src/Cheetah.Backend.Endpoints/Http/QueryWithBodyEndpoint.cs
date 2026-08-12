using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// POST endpoint that reads: the request arrives in the body, but the operation is a query.
///
/// Существует ради частого и вполне честного случая — чтения по списку идентификаторов: пачка Id
/// легко перерастает лимит длины URL, поэтому она едет телом, а метод остаётся POST. Обычные
/// <see cref="QueryEndpoint{TRequest,TQuery,TQueryResult,TResponse}"/> связывают запрос из строки
/// запроса и для такого не годятся, а
/// <see cref="CommandWithResultEndpoint{TRequest,TCommand,TCommandResult,TResponse}"/> потребовал бы
/// объявить чтение командой — то есть соврать о намерении ради формы.
///
/// Source generator will create: MapPost -> mapper.Map -> dispatcher.QueryAsync -> mapper.Map -> Results.Ok
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO (приезжает телом)</typeparam>
/// <typeparam name="TQuery">CQRS Query</typeparam>
/// <typeparam name="TQueryResult">Query result from handler</typeparam>
/// <typeparam name="TResponse">HTTP response DTO</typeparam>
public abstract class QueryWithBodyEndpoint<TRequest, TQuery, TQueryResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TQuery : IQuery<TQueryResult>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Post;
}

/// <summary>
/// POST endpoint that reads and answers with a collection.
///
/// Отдельная база, а не частный случай предыдущей, по той же причине, по какой
/// <see cref="QueryCollectionEndpoint{TRequest,TQuery,TQueryResult,TResponse}"/> отделён от
/// <see cref="QueryEndpoint{TRequest,TQuery,TQueryResult,TResponse}"/>: маркер
/// <see cref="ICrmResponse"/> носит ЭЛЕМЕНТ коллекции, а сама коллекция им быть не может — и без
/// этой пары ответ пришлось бы заворачивать в объект-обёртку, меняя контракт ради формы.
///
/// Source generator will create: MapPost -> dispatcher.QueryAsync -> map collection -> Results.Ok
/// </summary>
/// <typeparam name="TRequest">HTTP request DTO (приезжает телом)</typeparam>
/// <typeparam name="TQuery">CQRS Query, возвращающий коллекцию</typeparam>
/// <typeparam name="TQueryResult">Элемент результата запроса</typeparam>
/// <typeparam name="TResponse">Элемент ответа</typeparam>
public abstract class QueryCollectionWithBodyEndpoint<TRequest, TQuery, TQueryResult, TResponse>
    : EndpointBase<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TQuery : IQuery<IReadOnlyList<TQueryResult>>
    where TResponse : ICrmResponse
{
    public sealed override HttpMethod Method => HttpMethod.Post;
}
