using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;
using Crm.Recruitment.Domain;
using Mapster;

namespace Crm.Recruitment.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Vacancy (Entity → Model for Grid projection)
        config.NewConfig<Vacancy, VacancyModel>()
            .Map(dest => dest.StateColor, src => src.State != null && src.State.Color != null ? src.State.Color.Value : null)
            .Map(dest => dest.CustomerCode, src => src.Customer != null ? src.Customer.Code : null);

        config.NewConfig<VacancyModel, VacancyViewModel>();
        config.NewConfig<GetAllVacanciesGridRequest, GetVacanciesGridQuery>();
        config.NewConfig<GetVacancyByIdRequest, GetVacancyByIdQuery>();
        config.NewConfig<CreateVacancyRequest, CreateVacancyCommand>();
        config.NewConfig<UpdateVacancyRequest, UpdateVacancyCommand>();
        config.NewConfig<DeleteVacancyRequest, DeleteVacancyCommand>();
        config.NewConfig<MoveVacancyRequest, MoveVacancyCommand>();

        // VacancyState
        config.NewConfig<VacancyState, VacancyStateModel>()
            .Map(dest => dest.Color, src => src.Color != null ? src.Color.Value : null);
        config.NewConfig<VacancyStateModel, VacancyStateViewModel>();
        config.NewConfig<GetVacancyStateByIdRequest, GetVacancyStateByIdQuery>();
        config.NewConfig<GetAllVacancyStatesRequest, GetAllVacancyStatesQuery>();
        config.NewConfig<CreateVacancyStateRequest, CreateVacancyStateCommand>();
        config.NewConfig<UpdateVacancyStateRequest, UpdateVacancyStateCommand>();
        config.NewConfig<DeleteVacancyStateRequest, DeleteVacancyStateCommand>();

        config.NewConfig<VacancyRoleModel, VacancyRoleViewModel>();
        config.NewConfig<GetAllVacancyRolesRequest, GetAllVacancyRolesQuery>();

        // Customer
        config.NewConfig<CustomerModel, CustomerViewModel>();
        config.NewConfig<GetCustomerByIdRequest, GetCustomerByIdQuery>();
        config.NewConfig<GetAllCustomersRequest, GetAllCustomersQuery>();
        config.NewConfig<CreateCustomerRequest, CreateCustomerCommand>();
        config.NewConfig<UpdateCustomerRequest, UpdateCustomerCommand>();
        config.NewConfig<DeleteCustomerRequest, DeleteCustomerCommand>();

        // CustomerDirection
        config.NewConfig<CustomerDirectionModel, CustomerDirectionViewModel>();
        config.NewConfig<GetCustomerDirectionByIdRequest, GetCustomerDirectionByIdQuery>();
        config.NewConfig<GetAllCustomerDirectionsRequest, GetAllCustomerDirectionsQuery>();
        config.NewConfig<CreateCustomerDirectionRequest, CreateCustomerDirectionCommand>();
        config.NewConfig<UpdateCustomerDirectionRequest, UpdateCustomerDirectionCommand>();
        config.NewConfig<DeleteCustomerDirectionRequest, DeleteCustomerDirectionCommand>();

        // Position
        config.NewConfig<PositionModel, PositionViewModel>();
        config.NewConfig<GetPositionByIdRequest, GetPositionByIdQuery>();
        config.NewConfig<GetAllPositionsRequest, GetAllPositionsQuery>();
        config.NewConfig<CreatePositionRequest, CreatePositionCommand>();
        config.NewConfig<UpdatePositionRequest, UpdatePositionCommand>();
        config.NewConfig<DeletePositionRequest, DeletePositionCommand>();

        // StackItem
        config.NewConfig<StackItemModel, StackItemViewModel>();
        config.NewConfig<GetStackItemByIdRequest, GetStackItemByIdQuery>();
        config.NewConfig<GetAllStackItemsRequest, GetAllStackItemsQuery>();
        config.NewConfig<CreateStackItemRequest, CreateStackItemCommand>();
        config.NewConfig<UpdateStackItemRequest, UpdateStackItemCommand>();
        config.NewConfig<DeleteStackItemRequest, DeleteStackItemCommand>();

        // WorkFormat
        config.NewConfig<WorkFormatModel, WorkFormatViewModel>();
        config.NewConfig<GetWorkFormatByIdRequest, GetWorkFormatByIdQuery>();
        config.NewConfig<GetAllWorkFormatsRequest, GetAllWorkFormatsQuery>();
        config.NewConfig<CreateWorkFormatRequest, CreateWorkFormatCommand>();
        config.NewConfig<UpdateWorkFormatRequest, UpdateWorkFormatCommand>();
        config.NewConfig<DeleteWorkFormatRequest, DeleteWorkFormatCommand>();
    }
}