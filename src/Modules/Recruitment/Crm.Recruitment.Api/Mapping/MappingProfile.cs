using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;
using Mapster;

namespace Crm.Recruitment.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<VacancyModel, VacancyViewModel>();
        config.NewConfig<GetAllVacanciesGridRequest, GetVacanciesGridQuery>();
        config.NewConfig<GetVacancyByIdRequest, GetVacancyByIdQuery>();
        config.NewConfig<CreateVacancyRequest, CreateVacancyCommand>();
        config.NewConfig<UpdateVacancyRequest, UpdateVacancyCommand>();
        config.NewConfig<DeleteVacancyRequest, DeleteVacancyCommand>();

        config.NewConfig<VacancyRoleModel, VacancyRoleViewModel>();
        config.NewConfig<GetAllVacancyRolesRequest, GetAllVacancyRolesQuery>();
    }
}