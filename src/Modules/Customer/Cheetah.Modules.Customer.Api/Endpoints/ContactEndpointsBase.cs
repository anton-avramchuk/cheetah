using Cheetah.Core.CQRS;
using Cheetah.Modules.Customer.Application.Contacts;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Customer.Api.Endpoints;

/// <summary>
/// Абстрактная база CRUD-эндпоинтов контактных лиц клиента. Маршруты вложены в клиента:
/// <c>api/customers/{customerId}/contacts</c>. Generic над конкретными типами Contracts,
/// которые поставляет наследник; диспетчер резолвит handler по типу закрытой команды/запроса.
/// Отдельные маршруты можно переопределить (методы виртуальные).
/// </summary>
public abstract class ContactEndpointsBase<TCreateRequest, TUpdateRequest, TDto>
    where TCreateRequest : CreateContactRequestBase
    where TUpdateRequest : UpdateContactRequestBase
    where TDto : ContactDtoBase
{
    protected virtual string RoutePrefix =>
        $"{CustomerConstants.DefaultRoutePrefix}/{{customerId:guid}}/{CustomerConstants.DefaultContactsRouteSuffix}";

    public void Map(IEndpointRouteBuilder routes)
    {
        var prefix = RoutePrefix.TrimEnd('/');

        routes.MapPost(prefix, AddAsync).WithName("AddContact").WithTags("Contacts");
        routes.MapGet(prefix, ListAsync).WithName("ListContacts").WithTags("Contacts");
        routes.MapGet($"{prefix}/{{contactId:guid}}", GetByIdAsync).WithName("GetContactById").WithTags("Contacts");
        routes.MapPut($"{prefix}/{{contactId:guid}}", UpdateAsync).WithName("UpdateContact").WithTags("Contacts");
        routes.MapDelete($"{prefix}/{{contactId:guid}}", RemoveAsync).WithName("RemoveContact").WithTags("Contacts");
    }

    protected virtual async Task<IResult> AddAsync(
        [FromRoute] Guid customerId,
        [FromBody] TCreateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            var id = await dispatcher.SendAsync<AddContactCommand<TCreateRequest>, Guid>(
                new AddContactCommand<TCreateRequest>(customerId, request), ct);
            return Results.Created($"/{CustomerConstants.DefaultRoutePrefix}/{customerId}/{CustomerConstants.DefaultContactsRouteSuffix}/{id}", id);
        }
        catch (CustomerValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> ListAsync(
        [FromRoute] Guid customerId,
        [FromQuery] bool includeRemoved,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListContactsByCustomerQuery<TDto>, IReadOnlyList<TDto>>(
            new ListContactsByCustomerQuery<TDto>(customerId, includeRemoved), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> GetByIdAsync(
        [FromRoute] Guid contactId,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetContactByIdQuery<TDto>, TDto?>(
            new GetContactByIdQuery<TDto>(contactId), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual async Task<IResult> UpdateAsync(
        [FromRoute] Guid contactId,
        [FromBody] TUpdateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new UpdateContactCommand<TUpdateRequest>(contactId, request), ct);
            return Results.NoContent();
        }
        catch (CustomerValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> RemoveAsync(
        [FromRoute] Guid contactId,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new RemoveContactCommand(contactId), ct);
            return Results.NoContent();
        }
        catch (CustomerValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
