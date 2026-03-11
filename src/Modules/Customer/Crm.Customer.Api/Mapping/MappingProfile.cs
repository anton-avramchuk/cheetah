using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Customer.Application;
using Crm.Customer.Application.Commands;
using Crm.Customer.Application.Queries;
using Crm.Customer.Contracts.Requests;
using Crm.Customer.Contracts.Response;
using Mapster;

namespace Crm.Customer.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerModel, CustomerViewModel>();
        config.NewConfig<GetAllSampleEntitiesRequest, GetAllSampleEntitiesQuery>();
        config.NewConfig<GetCustomerByIdRequest, GetCustomerByIdQuery>();
        config.NewConfig<CreateCustomerRequest, CreateCustomerCommand>();
        config.NewConfig<UpdateCustomerRequest, UpdateCustomerCommand>();
        config.NewConfig<DeleteCustomerRequest, DeleteCustomerCommand>();
    }
}