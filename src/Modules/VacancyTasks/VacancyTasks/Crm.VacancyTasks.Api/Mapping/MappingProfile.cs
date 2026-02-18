using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.VacancyTasks.Application;
using Crm.VacancyTasks.Application.Commands;
using Crm.VacancyTasks.Application.Queries;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;
using Crm.VacancyTasks.Domain;
using Mapster;

namespace Crm.VacancyTasks.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // VacancyTask (Entity → Model for Grid projection)
        config.NewConfig<VacancyTask, VacancyTaskModel>()
            .Map(dest => dest.StateName, src => src.State.Name)
            .Map(dest => dest.PriorityName, src => src.Priority != null ? src.Priority.Name : null)
            .Map(dest => dest.PriorityColor, src => src.Priority != null && src.Priority.Color != null ? src.Priority.Color.Value : null);

        config.NewConfig<VacancyTaskModel, VacancyTaskViewModel>();
        config.NewConfig<GetAllVacancyTasksRequest, GetAllVacancyTasksQuery>();
        config.NewConfig<GetVacancyTaskByIdRequest, GetVacancyTaskByIdQuery>();
        config.NewConfig<CreateVacancyTaskRequest, CreateVacancyTaskCommand>();
        config.NewConfig<UpdateVacancyTaskRequest, UpdateVacancyTaskCommand>();
        config.NewConfig<DeleteVacancyTaskRequest, DeleteVacancyTaskCommand>();
        config.NewConfig<MoveVacancyTaskRequest, MoveVacancyTaskCommand>();

        // TaskState
        config.NewConfig<TaskStateModel, TaskStateViewModel>();
        config.NewConfig<GetTaskStateByIdRequest, GetTaskStateByIdQuery>();
        config.NewConfig<GetAllTaskStatesRequest, GetAllTaskStatesQuery>();
        config.NewConfig<CreateTaskStateRequest, CreateTaskStateCommand>();
        config.NewConfig<UpdateTaskStateRequest, UpdateTaskStateCommand>();
        config.NewConfig<DeleteTaskStateRequest, DeleteTaskStateCommand>();

        // TaskPriority
        config.NewConfig<TaskPriorityModel, TaskPriorityViewModel>();
        config.NewConfig<GetTaskPriorityByIdRequest, GetTaskPriorityByIdQuery>();
        config.NewConfig<GetAllTaskPrioritiesRequest, GetAllTaskPrioritiesQuery>();
        config.NewConfig<CreateTaskPriorityRequest, CreateTaskPriorityCommand>();
        config.NewConfig<UpdateTaskPriorityRequest, UpdateTaskPriorityCommand>();
        config.NewConfig<DeleteTaskPriorityRequest, DeleteTaskPriorityCommand>();
    }
}
