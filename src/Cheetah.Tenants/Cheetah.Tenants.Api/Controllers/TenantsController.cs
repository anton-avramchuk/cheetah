using Cheetah.Core.CQRS;
using Cheetah.Tenants.Application.Commands;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Shared.Requests;
using Cheetah.Tenants.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cheetah.Tenants.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public TenantsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TenantViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _dispatcher.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Domain.Entities.Tenant>>(
            new GetAllTenantsQuery(),
            cancellationToken);

        var viewModels = tenants.Select(t => new TenantViewModel
        {
            Id = t.Id,
            Name = t.Name,
            Subdomain = t.Subdomain,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            ConnectionStrings = t.ConnectionStrings.Select(cs => new TenantConnectionStringViewModel
            {
                Id = cs.Id,
                Name = cs.Name,
                ConnectionString = cs.ConnectionString,
                IsDefault = cs.IsDefault
            }).ToList()
        }).ToList();

        return Ok(viewModels);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _dispatcher.QueryAsync<GetTenantByIdQuery, Domain.Entities.Tenant?>(
            new GetTenantByIdQuery(id),
            cancellationToken);

        if (tenant == null)
            return NotFound();

        var viewModel = new TenantViewModel
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            ConnectionStrings = tenant.ConnectionStrings.Select(cs => new TenantConnectionStringViewModel
            {
                Id = cs.Id,
                Name = cs.Name,
                ConnectionString = cs.ConnectionString,
                IsDefault = cs.IsDefault
            }).ToList()
        };

        return Ok(viewModel);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTenantCommand(request.Name, request.Subdomain);
        var tenantId = await _dispatcher.SendAsync<CreateTenantCommand, Guid>(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = tenantId }, tenantId);
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ActivateTenantCommand(id);
            await _dispatcher.SendAsync(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeactivateTenantCommand(id);
            await _dispatcher.SendAsync(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
