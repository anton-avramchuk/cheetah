using Cheetah.Core.CQRS;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Application.Customers;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Customer.Application.Extensions;

public static class CustomerApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы конкретной реализации.
    /// Вызывается из прикладного модуля наследника после <c>AddCustomerInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddCustomerApplication<TCustomer, TCreateRequest, TUpdateRequest, TDto, TFactory, TProjector>(
        this IServiceCollection services)
        where TCustomer : CustomerBase
        where TCreateRequest : CreateCustomerRequestBase
        where TUpdateRequest : UpdateCustomerRequestBase
        where TDto : CustomerDtoBase
        where TFactory : class, ICustomerFactory<TCustomer, TCreateRequest>
        where TProjector : class, ICustomerProjector<TCustomer, TDto>
    {
        services.AddScoped<ICustomerFactory<TCustomer, TCreateRequest>, TFactory>();
        services.AddScoped<ICustomerProjector<TCustomer, TDto>, TProjector>();

        services.AddScoped<ICommandHandler<CreateCustomerCommand<TCreateRequest>, Guid>,
            CreateCustomerCommandHandler<TCustomer, TCreateRequest>>();
        services.AddScoped<ICommandHandler<UpdateCustomerCommand<TUpdateRequest>>,
            UpdateCustomerCommandHandler<TCustomer, TUpdateRequest>>();
        services.AddScoped<ICommandHandler<ArchiveCustomerCommand>,
            ArchiveCustomerCommandHandler<TCustomer>>();
        services.AddScoped<IQueryHandler<GetCustomerByIdQuery<TDto>, TDto?>,
            GetCustomerByIdQueryHandler<TCustomer, TDto>>();
        services.AddScoped<IQueryHandler<ListCustomersQuery<TDto>, IReadOnlyList<TDto>>,
            ListCustomersQueryHandler<TCustomer, TDto>>();

        return services;
    }
}
