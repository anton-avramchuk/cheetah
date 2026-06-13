using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Customer.Api.Grpc;
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
        config.NewConfig<GetSampleEntitiesGridRequest, GetSampleEntitiesGridQuery>();
        config.NewConfig<GetCustomerByIdRequest, GetCustomerByIdQuery>();
        config.NewConfig<CreateCustomerRequest, CreateCustomerCommand>();
        config.NewConfig<UpdateCustomerRequest, UpdateCustomerCommand>();
        config.NewConfig<DeleteCustomerRequest, DeleteCustomerCommand>();

        // gRPC (proto <-> CQRS)
        config.NewConfig<GetCustomerByIdGrpcRequest, GetCustomerByIdQuery>()
            .Map(d => d.Id, s => System.Guid.Parse(s.Id));

        // optional proto-строки нельзя присваивать null — ставим их только при наличии (AfterMapping)
        config.NewConfig<CustomerModel, CustomerGrpcReply>()
            .Map(d => d.Id, s => s.Id.ToString())
            .Ignore(d => d.Description)
            .Ignore(d => d.IndustryId)
            .AfterMapping((s, d) =>
            {
                if (s.Description != null) d.Description = s.Description;
                if (s.IndustryId != null) d.IndustryId = s.IndustryId.Value.ToString();
            });

        config.NewConfig<CreateCustomerGrpcRequest, CreateCustomerCommand>()
            .Map(d => d.Description, s => s.HasDescription ? s.Description : null)
            .Map(d => d.IndustryId, s => s.HasIndustryId ? (System.Guid?)System.Guid.Parse(s.IndustryId) : null);

        config.NewConfig<System.Guid, CreateCustomerGrpcReply>()
            .MapWith(s => new CreateCustomerGrpcReply { Id = s.ToString() });
    }
}