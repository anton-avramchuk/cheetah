using Cheetah.Core.CQRS;
using Cheetah.Modules.Leads.Application.Abstractions;
using Cheetah.Modules.Leads.Application.Leads;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Leads.Application.Extensions;

public static class LeadsApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы конкретной реализации.
    /// Вызывается из прикладного модуля наследника после <c>AddLeadsInfrastructure</c>.
    /// Реализацию <c>ILeadConversionOrchestrator</c> наследник регистрирует отдельно.
    /// </summary>
    public static IServiceCollection AddLeadsApplication<TLead, TCreateRequest, TUpdateRequest, TConvertRequest, TDto, TFactory, TProjector>(
        this IServiceCollection services)
        where TLead : LeadBase
        where TCreateRequest : CreateLeadRequestBase
        where TUpdateRequest : UpdateLeadRequestBase
        where TConvertRequest : ConvertLeadRequestBase
        where TDto : LeadDtoBase
        where TFactory : class, ILeadFactory<TLead, TCreateRequest>
        where TProjector : class, ILeadProjector<TLead, TDto>
    {
        services.AddScoped<ILeadFactory<TLead, TCreateRequest>, TFactory>();
        services.AddScoped<ILeadProjector<TLead, TDto>, TProjector>();

        services.AddScoped<ICommandHandler<CreateLeadCommand<TCreateRequest>, Guid>,
            CreateLeadCommandHandler<TLead, TCreateRequest>>();
        services.AddScoped<ICommandHandler<UpdateLeadCommand<TUpdateRequest>>,
            UpdateLeadCommandHandler<TLead, TUpdateRequest>>();
        services.AddScoped<ICommandHandler<QualifyLeadCommand>,
            QualifyLeadCommandHandler<TLead>>();
        services.AddScoped<ICommandHandler<DisqualifyLeadCommand>,
            DisqualifyLeadCommandHandler<TLead>>();
        services.AddScoped<ICommandHandler<ConvertLeadCommand<TConvertRequest>, ConvertLeadResult>,
            ConvertLeadCommandHandler<TLead, TConvertRequest>>();
        services.AddScoped<IQueryHandler<GetLeadByIdQuery<TDto>, TDto?>,
            GetLeadByIdQueryHandler<TLead, TDto>>();
        services.AddScoped<IQueryHandler<ListLeadsQuery<TDto>, IReadOnlyList<TDto>>,
            ListLeadsQueryHandler<TLead, TDto>>();

        return services;
    }
}
