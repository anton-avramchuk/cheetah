using Cheetah.Core.CQRS;
using Cheetah.Modules.Customer.Application.Customers;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Customer.Api.Endpoints;

/// <summary>
/// Абстрактная база CRUD-эндпоинтов клиента. Generic над конкретными типами Contracts,
/// которые поставляет наследник. Маршруты строятся на закрытых generic-командах/запросах,
/// поэтому конкретный <c>TCustomer</c> здесь не нужен — диспетчер резолвит handler по типу
/// команды. Отдельные маршруты можно переопределить (методы виртуальные).
/// </summary>
public abstract class CustomerEndpointsBase<TCreateRequest, TUpdateRequest, TDto>
    where TCreateRequest : CreateCustomerRequestBase
    where TUpdateRequest : UpdateCustomerRequestBase
    where TDto : CustomerDtoBase
{
    protected virtual string RoutePrefix => CustomerConstants.DefaultRoutePrefix;

    public void Map(IEndpointRouteBuilder routes)
    {
        var prefix = RoutePrefix.TrimEnd('/');

        routes.MapPost(prefix, CreateAsync).WithName("CreateCustomer").WithTags("Customers");
        routes.MapGet($"{prefix}/{{id:guid}}", GetByIdAsync).WithName("GetCustomerById").WithTags("Customers");
        routes.MapGet(prefix, ListAsync).WithName("ListCustomers").WithTags("Customers");
        routes.MapPut($"{prefix}/{{id:guid}}", UpdateAsync).WithName("UpdateCustomer").WithTags("Customers");
        routes.MapDelete($"{prefix}/{{id:guid}}", ArchiveAsync).WithName("ArchiveCustomer").WithTags("Customers");
    }

    protected virtual async Task<IResult> CreateAsync(
        [FromBody] TCreateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            var id = await dispatcher.SendAsync<CreateCustomerCommand<TCreateRequest>, Guid>(
                new CreateCustomerCommand<TCreateRequest>(request), ct);
            return Results.Created($"/{RoutePrefix.TrimEnd('/')}/{id}", id);
        }
        catch (CustomerValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetCustomerByIdQuery<TDto>, TDto?>(
            new GetCustomerByIdQuery<TDto>(id), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual async Task<IResult> ListAsync(
        [FromQuery] bool activeOnly,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListCustomersQuery<TDto>, IReadOnlyList<TDto>>(
            new ListCustomersQuery<TDto>(activeOnly), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] TUpdateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new UpdateCustomerCommand<TUpdateRequest>(id, request), ct);
            return Results.NoContent();
        }
        catch (CustomerValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> ArchiveAsync(
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new ArchiveCustomerCommand(id), ct);
            return Results.NoContent();
        }
        catch (CustomerValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
