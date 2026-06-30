using Cheetah.Core.CQRS;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Application.Contacts;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Customer.Application.Extensions;

public static class CustomerContactsServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы контактных лиц
    /// конкретной реализации. Вызывается из прикладного модуля наследника наряду с
    /// <c>AddCustomerApplication</c> (контакты — опциональны).
    /// </summary>
    public static IServiceCollection AddCustomerContacts<TContact, TCreateRequest, TUpdateRequest, TDto, TFactory, TProjector, TPosition>(
        this IServiceCollection services)
        where TContact : ContactBase<TPosition>
        where TCreateRequest : CreateContactRequestBase
        where TUpdateRequest : UpdateContactRequestBase
        where TDto : ContactDtoBase
        where TFactory : class, IContactFactory<TContact, TCreateRequest, TPosition>
        where TProjector : class, IContactProjector<TContact, TDto, TPosition>
        where TPosition : PositionBase
    {
        services.AddScoped<IContactFactory<TContact, TCreateRequest, TPosition>, TFactory>();
        services.AddScoped<IContactProjector<TContact, TDto, TPosition>, TProjector>();

        services.AddScoped<ICommandHandler<AddContactCommand<TCreateRequest>, Guid>,
            AddContactCommandHandler<TContact, TCreateRequest, TPosition>>();
        services.AddScoped<ICommandHandler<UpdateContactCommand<TUpdateRequest>>,
            UpdateContactCommandHandler<TContact, TUpdateRequest, TPosition>>();
        services.AddScoped<ICommandHandler<RemoveContactCommand>,
            RemoveContactCommandHandler<TContact, TPosition>>();
        services.AddScoped<IQueryHandler<GetContactByIdQuery<TDto>, TDto?>,
            GetContactByIdQueryHandler<TContact, TDto, TPosition>>();
        services.AddScoped<IQueryHandler<ListContactsByCustomerQuery<TDto>, IReadOnlyList<TDto>>,
            ListContactsByCustomerQueryHandler<TContact, TDto, TPosition>>();

        return services;
    }
}
