using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.VacancyTasks.Application;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Application.Queries;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;
using Mapster;

namespace Crm.VacancyTasks.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<VacancyTaskModel, VacancyTaskViewModel>();
        config.NewConfig<GetAllSampleEntitiesRequest, GetAllSampleEntitiesQuery>();
        config.NewConfig<GetVacancyTaskByIdRequest, GetVacancyTaskByIdQuery>();
        config.NewConfig<CreateVacancyTaskRequest, CreateVacancyTaskCommand>();
        config.NewConfig<UpdateVacancyTaskRequest, UpdateVacancyTaskCommand>();
        config.NewConfig<DeleteVacancyTaskRequest, DeleteVacancyTaskCommand>();
    }
}