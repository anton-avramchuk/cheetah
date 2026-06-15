using Cheetah.Core.CQRS;
using Cheetah.Modules.Leads.Application.Exceptions;
using Cheetah.Modules.Leads.Application.Leads;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Leads.Api.Endpoints;

/// <summary>
/// Абстрактная база CRUD/lifecycle-эндпоинтов лида. Generic над конкретными типами Contracts, которые
/// поставляет наследник. Маршруты строятся на закрытых generic-командах/запросах — конкретный тип
/// сущности здесь не нужен. Любой маршрут можно переопределить (методы виртуальные).
/// </summary>
public abstract class LeadEndpointsBase<TCreateRequest, TUpdateRequest, TConvertRequest, TDto>
    where TCreateRequest : CreateLeadRequestBase
    where TUpdateRequest : UpdateLeadRequestBase
    where TConvertRequest : ConvertLeadRequestBase
    where TDto : LeadDtoBase
{
    protected virtual string RoutePrefix => LeadsConstants.DefaultRoutePrefix;

    public void Map(IEndpointRouteBuilder routes)
    {
        var prefix = RoutePrefix.TrimEnd('/');

        routes.MapPost(prefix, CreateAsync).WithName("CreateLead").WithTags("Leads");
        routes.MapGet(prefix, ListAsync).WithName("ListLeads").WithTags("Leads");
        routes.MapGet($"{prefix}/{{id:guid}}", GetByIdAsync).WithName("GetLeadById").WithTags("Leads");
        routes.MapPut($"{prefix}/{{id:guid}}", UpdateAsync).WithName("UpdateLead").WithTags("Leads");
        routes.MapPost($"{prefix}/{{id:guid}}/qualify", QualifyAsync).WithName("QualifyLead").WithTags("Leads");
        routes.MapPost($"{prefix}/{{id:guid}}/disqualify", DisqualifyAsync).WithName("DisqualifyLead").WithTags("Leads");
        routes.MapPost($"{prefix}/{{id:guid}}/convert", ConvertAsync).WithName("ConvertLead").WithTags("Leads");
    }

    protected virtual async Task<IResult> CreateAsync(
        [FromBody] TCreateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            var id = await dispatcher.SendAsync<CreateLeadCommand<TCreateRequest>, Guid>(
                new CreateLeadCommand<TCreateRequest>(request), ct);
            return Results.Created($"/{RoutePrefix.TrimEnd('/')}/{id}", id);
        }
        catch (LeadValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> ListAsync(
        [FromQuery] LeadStatus? status,
        [FromQuery] LeadSource? source,
        [FromQuery] Guid? ownerId,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var items = await dispatcher.QueryAsync<ListLeadsQuery<TDto>, IReadOnlyList<TDto>>(
            new ListLeadsQuery<TDto>(status, source, ownerId), ct);
        return Results.Ok(items);
    }

    protected virtual async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        var dto = await dispatcher.QueryAsync<GetLeadByIdQuery<TDto>, TDto?>(
            new GetLeadByIdQuery<TDto>(id), ct);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    protected virtual async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] TUpdateRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new UpdateLeadCommand<TUpdateRequest>(id, request), ct);
            return Results.NoContent();
        }
        catch (LeadValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> QualifyAsync(
        [FromRoute] Guid id,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new QualifyLeadCommand(id), ct);
            return Results.NoContent();
        }
        catch (LeadValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> DisqualifyAsync(
        [FromRoute] Guid id,
        [FromBody] DisqualifyLeadBody body,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            await dispatcher.SendAsync(new DisqualifyLeadCommand(id, body.Reason), ct);
            return Results.NoContent();
        }
        catch (LeadValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    protected virtual async Task<IResult> ConvertAsync(
        [FromRoute] Guid id,
        [FromBody] TConvertRequest request,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct)
    {
        try
        {
            var result = await dispatcher.SendAsync<ConvertLeadCommand<TConvertRequest>, ConvertLeadResult>(
                new ConvertLeadCommand<TConvertRequest>(id, request), ct);
            return Results.Ok(result);
        }
        catch (LeadValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>Тело запроса дисквалификации.</summary>
public sealed record DisqualifyLeadBody(string Reason);
