using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Candidates.Application;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Application.Queries;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;
using Mapster;

namespace Crm.Candidates.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<CandidateModel, CandidateViewModel>();
        config.NewConfig<GetAllCandidatesRequest, GetAllCandidatesQuery>();
        config.NewConfig<GetCandidateByIdRequest, GetCandidateByIdQuery>();
        config.NewConfig<CreateCandidateRequest, CreateCandidateCommand>();
        config.NewConfig<UpdateCandidateRequest, UpdateCandidateCommand>();
        config.NewConfig<DeleteCandidateRequest, DeleteCandidateCommand>();
    }
}